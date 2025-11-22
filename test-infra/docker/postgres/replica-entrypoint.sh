#!/bin/bash
set -e

if [ ! -s "$PGDATA/PG_VERSION" ]; then
    echo ">>> Data directory is empty. Starting base backup from Master (db)..."
    
    until PGPASSWORD=$POSTGRES_PASSWORD psql -h db -U postgres -c '\q'; do
        echo ">>> Waiting for master database to be ready..."
        sleep 2
    done

    echo ">>> Master is online. Cloning data..."
    
    PGPASSWORD=replicatorpass pg_basebackup -h db -D ${PGDATA} -U replicator -v -P -R -X stream

    echo ">>> Backup completed. Fixing permissions..."
    chmod 0700 ${PGDATA}
fi

exec docker-entrypoint.sh "$@"