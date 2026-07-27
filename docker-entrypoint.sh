#!/bin/sh
set -eu

read_secret() {
  variable_name="$1"
  secret_path="$2"
  if [ -f "$secret_path" ]; then
    # PowerShell's `echo ... | docker secret create ... -` stores CRLF.
    # HTTP headers reject those invisible characters, so strip only line endings.
    export "$variable_name=$(tr -d '\r\n' < "$secret_path")"
  fi
}

read_secret POSTGRES_PASSWORD /run/secrets/postgres_password
read_secret EmailSettings__Password /run/secrets/smtp_password
read_secret EmailSettings__SenderEmail /run/secrets/email_sender_email
read_secret PaymentSettings__PayPal__ClientId /run/secrets/paypal_client_id
read_secret PaymentSettings__PayPal__ClientSecret /run/secrets/paypal_client_secret
read_secret PayPhone__Token /run/secrets/payphone_token
read_secret PayPhone__StoreId /run/secrets/payphone_store_id
read_secret Authentication__Google__ClientId /run/secrets/google_client_id
read_secret Authentication__Google__ClientSecret /run/secrets/google_client_secret

if [ -n "${POSTGRES_PASSWORD:-}" ]; then
  export ConnectionStrings__DefaultConnection="Host=${POSTGRES_HOST:-postgres};Port=5432;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}"
fi

exec dotnet HuellitasFelices.dll
