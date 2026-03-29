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
REMOTE_TMP_DIR="${REMOTE_TMP_DIR:-/tmp/marketstat-nginx-test}"
LOCAL_NGINX_CONF="${LOCAL_NGINX_CONF:-deploy/nginx/marketstat.conf}"

if [[ ! -f "${LOCAL_NGINX_CONF}" ]]; then
  echo "[test-nginx] Missing local nginx config: ${LOCAL_NGINX_CONF}"
  exit 1
fi

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

echo "[test-nginx] Preparing remote temp directory..."
ssh "${SSH_OPTS[@]}" "${DEPLOY_USER}@${DEPLOY_HOST}" "rm -rf '${REMOTE_TMP_DIR}' && mkdir -p '${REMOTE_TMP_DIR}'"

echo "[test-nginx] Uploading candidate nginx config..."
scp "${SCP_OPTS[@]}" "${LOCAL_NGINX_CONF}" "${DEPLOY_USER}@${DEPLOY_HOST}:${REMOTE_TMP_DIR}/marketstat.conf"

echo "[test-nginx] Creating temporary nginx root config..."
ssh "${SSH_OPTS[@]}" "${DEPLOY_USER}@${DEPLOY_HOST}" "cat > '${REMOTE_TMP_DIR}/nginx-test.conf' <<'NGINXCONF'
events {}

http {
    include /etc/nginx/mime.types;
    default_type application/octet-stream;
    include ${REMOTE_TMP_DIR}/marketstat.conf;
}
NGINXCONF"

echo "[test-nginx] Running nginx syntax test on VM1..."
ssh "${SSH_OPTS[@]}" "${DEPLOY_USER}@${DEPLOY_HOST}" "sudo -n /usr/sbin/nginx -t -c '${REMOTE_TMP_DIR}/nginx-test.conf' -g 'pid ${REMOTE_TMP_DIR}/nginx-test.pid; error_log stderr;'"

echo "[test-nginx] Remote nginx syntax validation completed successfully."
