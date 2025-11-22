FROM postgres:16-alpine

ENV POSTGRES_DB=marketstat
ENV POSTGRES_PASSWORD=postgres

COPY 01-init.sql /docker-entrypoint-initdb.d/

RUN echo "host replication replicator 0.0.0.0/0 md5" >> /usr/local/share/postgresql/pg_hba.conf.sample