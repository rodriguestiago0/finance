create table if not exists ticker
(
    ticker_id          serial
        constraint ticker_pk
            primary key,
    external_ticker_id uuid    not null,
    short_id           varchar not null,
    ticker_type        integer not null,
    name               varchar not null,
    isin               varchar not null,
    exchange           varchar not null,
    currency           integer not null
);

create unique index if not exists ticker_external_ticker_id_uindex
    on ticker (external_ticker_id);

create unique index if not exists ticker_ticker_id_uindex
    on ticker (ticker_id);

