#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
cd "$ROOT_DIR"

: "${DEPLOY_HOST:?DEPLOY_HOST is required}"
: "${DEPLOY_USER:?DEPLOY_USER is required}"
: "${SSH_PRIVATE_KEY_B64:?SSH_PRIVATE_KEY_B64 is required}"

SSH_KEY_FILE="$(mktemp)"
trap 'rm -f "${SSH_KEY_FILE}"' EXIT

printf '%s' "${SSH_PRIVATE_KEY_B64}" | base64 -d > "${SSH_KEY_FILE}"
chmod 600 "${SSH_KEY_FILE}"

if ! ssh-keygen -y -f "${SSH_KEY_FILE}" >/dev/null 2>&1; then
  echo "[ssh] Invalid decoded private key"
  exit 1
fi

DEPLOY_PORT="${DEPLOY_PORT:-22}"
RELEASE_ID="${RELEASE_ID:-$(date -u +%Y%m%d%H%M%S)}"
REMOTE_BASE_DIR="${REMOTE_BASE_DIR:-/opt/marketstat}"
REMOTE_RELEASE_DIR="${REMOTE_BASE_DIR}/releases/${RELEASE_ID}"
REMOTE_CURRENT_LINK="${REMOTE_BASE_DIR}/current"
LOCAL_NGINX_CONF="${LOCAL_NGINX_CONF:-deploy/nginx/marketstat.conf}"
REMOTE_NGINX_TARGET="${REMOTE_NGINX_TARGET:-/etc/nginx/sites-available/default}"

if [[ ! -d artifacts/publish ]]; then
  echo "[deploy] Missing artifacts/publish. Run build first."
  exit 1
fi

if [[ ! -f "${LOCAL_NGINX_CONF}" ]]; then
  echo "[deploy] Missing nginx config: ${LOCAL_NGINX_CONF}"
  exit 1
fi

ARCHIVE_PATH="artifacts/marketstat-${RELEASE_ID}.tar.gz"

echo "[deploy] Creating archive ${ARCHIVE_PATH}..."
tar -C artifacts/publish -czf "${ARCHIVE_PATH}" .

SSH_OPTS=(
  -i "${SSH_KEY_FILE}"
  -p "${DEPLOY_PORT}"
  -o StrictHostKeyChecking=accept-new
)

SCP_OPTS=(
  -i "${SSH_KEY_FILE}"
  -P "${DEPLOY_PORT}"
  -o StrictHostKeyChecking=accept-new
)

echo "[deploy] Creating remote release directory..."
ssh "${SSH_OPTS[@]}" "${DEPLOY_USER}@${DEPLOY_HOST}" "mkdir -p '${REMOTE_RELEASE_DIR}'"

echo "[deploy] Uploading application archive..."
scp "${SCP_OPTS[@]}" "${ARCHIVE_PATH}" "${DEPLOY_USER}@${DEPLOY_HOST}:${REMOTE_RELEASE_DIR}/app.tar.gz"

echo "[deploy] Uploading nginx config..."
scp "${SCP_OPTS[@]}" "${LOCAL_NGINX_CONF}" "${DEPLOY_USER}@${DEPLOY_HOST}:${REMOTE_RELEASE_DIR}/marketstat.conf"

echo "[deploy] Extracting release, switching symlink, updating nginx, restarting app..."
ssh "${SSH_OPTS[@]}" "${DEPLOY_USER}@${DEPLOY_HOST}" "
  set -euo pipefail
  tar -xzf '${REMOTE_RELEASE_DIR}/app.tar.gz' -C '${REMOTE_RELEASE_DIR}'
  rm -f '${REMOTE_RELEASE_DIR}/app.tar.gz'
  ln -sfn '${REMOTE_RELEASE_DIR}' '${REMOTE_CURRENT_LINK}'
  sudo -n /usr/bin/install -o root -g root -m 644 '${REMOTE_RELEASE_DIR}/marketstat.conf' '${REMOTE_NGINX_TARGET}'
  sudo -n /usr/sbin/nginx -t
  sudo -n /usr/bin/systemctl reload nginx
  sudo -n /usr/bin/systemctl restart marketstat
  sleep 3
  sudo -n /usr/bin/systemctl status marketstat >/dev/null
"

echo "[deploy] Deployment completed successfully."
