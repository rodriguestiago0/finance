create table if not exists finance.ticker
(
    ticker_id   uuid default gen_random_uuid() not null
        constraint ticker_pk
            primary key,
    short_id    varchar                        not null,
    ticker_type integer                        not null,
    name        varchar                        not null,
    isin        varchar                        not null,
    exchange    varchar                        not null,
    currency    integer                        not null
);

create unique index if not exists ticker_ticker_id_uindex
    on finance.ticker (ticker_id);

create unique index if not exists ticker_isin_uindex
    on finance.ticker (isin);

