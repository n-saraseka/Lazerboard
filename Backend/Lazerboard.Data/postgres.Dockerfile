FROM postgres:18

# Install PGDG repository
RUN apt-get update && apt-get install -y \
wget \
gnupg \
lsb-release \
&& wget --quiet -O /usr/share/keyrings/postgresql-archive-keyring.asc https://www.postgresql.org/media/keys/ACCC4CF8.asc \
&& echo "deb [signed-by=/usr/share/keyrings/postgresql-archive-keyring.asc] http://apt.postgresql.org/pub/repos/apt $(lsb_release -cs)-pgdg main" > /etc/apt/sources.list.d/pgdg.list \
&& apt-get update

# Install pgBackRest from PGDG repository
RUN apt-get install -y pgbackrest \
&& rm -rf /var/lib/apt/lists/*

# Enable archive_mode + archive_command on first initdb
RUN mkdir -p /docker-entrypoint-initdb.d && \
cat >/docker-entrypoint-initdb.d/pgbackrest-archive.sh <<'EOF'
#!/bin/bash
set -e

echo "archive_mode = on" >> "$PGDATA/postgresql.auto.conf"
echo "archive_command = 'pgbackrest --stanza=main archive-push %p'" >> "$PGDATA/postgresql.auto.conf"
EOF
RUN chmod +x /docker-entrypoint-initdb.d/pgbackrest-archive.sh

EXPOSE 5432