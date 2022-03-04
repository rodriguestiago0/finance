create table ticker
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
    currency           integer not null,
    taxation_required  boolean not null
);

create unique index ticker_external_ticker_id_uindex
    on ticker (external_ticker_id);

create unique index ticker_ticker_id_uindex
    on ticker (ticker_id);

create unique index ticker_isin_uindex
    on ticker (isin);


