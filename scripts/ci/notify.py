#!/usr/bin/env python3
from __future__ import annotations

import json
import os
import smtplib
import ssl
import sys
import urllib.parse
import urllib.request
from email.message import EmailMessage


def require_env(name: str) -> str:
    value = os.environ.get(name)
    if not value:
        print(f"[notify] Missing required environment variable: {name}", file=sys.stderr)
        sys.exit(1)
    return value


def api_get(url: str, headers: dict[str, str]) -> list[dict]:
    request = urllib.request.Request(url, headers=headers, method="GET")
    with urllib.request.urlopen(request, timeout=30) as response:
        charset = response.headers.get_content_charset() or "utf-8"
        return json.loads(response.read().decode(charset))


def send_email(
    smtp_host: str,
    smtp_port: int,
    smtp_username: str,
    smtp_password: str,
    mail_from: str,
    mail_to: str,
    subject: str,
    body: str,
) -> None:
    message = EmailMessage()
    message["From"] = mail_from
    message["To"] = mail_to
    message["Subject"] = subject
    message.set_content(body)

    context = ssl.create_default_context()

    with smtplib.SMTP(smtp_host, smtp_port, timeout=30) as smtp:
        smtp.ehlo()
        smtp.starttls(context=context)
        smtp.ehlo()
        smtp.login(smtp_username, smtp_password)
        smtp.send_message(message)


def main() -> int:
    ci_api_v4_url = require_env("CI_API_V4_URL")
    ci_project_id = require_env("CI_PROJECT_ID")
    ci_pipeline_id = require_env("CI_PIPELINE_ID")
    ci_project_path = require_env("CI_PROJECT_PATH")
    ci_pipeline_url = require_env("CI_PIPELINE_URL")
    ci_commit_ref_name = require_env("CI_COMMIT_REF_NAME")
    ci_commit_short_sha = require_env("CI_COMMIT_SHORT_SHA")
    ci_job_token = require_env("CI_JOB_TOKEN")

    smtp_host = require_env("SMTP_HOST")
    smtp_port = int(require_env("SMTP_PORT"))
    smtp_username = require_env("SMTP_USERNAME")
    smtp_password = require_env("SMTP_PASSWORD")
    smtp_from = require_env("SMTP_FROM")
    smtp_to = require_env("SMTP_TO")

    jobs_url = (
        f"{ci_api_v4_url}/projects/{urllib.parse.quote(ci_project_id, safe='')}"
        f"/pipelines/{urllib.parse.quote(ci_pipeline_id, safe='')}/jobs"
        "?per_page=100&include_retried=true"
    )

    jobs = api_get(
        jobs_url,
        headers={
            "JOB-TOKEN": ci_job_token,
            "Accept": "application/json",
        },
    )

    jobs_sorted = sorted(
        jobs,
        key=lambda item: (
            str(item.get("stage", "")),
            int(item.get("id", 0)),
        ),
    )

    subject = f"MarketStat pipeline: {ci_commit_ref_name} ({ci_commit_short_sha})"

    lines: list[str] = [
        "MarketStat pipeline result",
        f"Project: {ci_project_path}",
        f"Branch: {ci_commit_ref_name}",
        f"Commit: {ci_commit_short_sha}",
        f"Pipeline: {ci_pipeline_url}",
        "",
        "Jobs:",
    ]

    for job in jobs_sorted:
        stage = str(job.get("stage", "unknown"))
        name = str(job.get("name", "unknown"))
        status = str(job.get("status", "unknown"))
        lines.append(f"[{stage}] {name}: {status}")

    body = "\n".join(lines)

    print("[notify] Sending email notification...")
    print(body)

    send_email(
        smtp_host=smtp_host,
        smtp_port=smtp_port,
        smtp_username=smtp_username,
        smtp_password=smtp_password,
        mail_from=smtp_from,
        mail_to=smtp_to,
        subject=subject,
        body=body,
    )

    print("[notify] Notification sent successfully.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
