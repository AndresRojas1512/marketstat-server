FROM postgres:16-alpine

ENV POSTGRES_DB=marketstat
ENV POSTGRES_PASSWORD=postgres

COPY 01-init.sql /docker-entrypoint-initdb.d/