create table if not exists finance.transaction
(
    transaction_id        uuid default gen_random_uuid() not null
        constraint transaction_pk
            primary key,
    broker_transaction_id varchar,
    ticker_id             uuid                           not null
        constraint transaction_ticker_ticker_id_fk
            references finance.ticker
            on delete cascade,
    date                  timestamp with time zone       not null,
    units                 numeric                        not null,
    price                 numeric                        not null,
    local_price           numeric,
    fee                   numeric,
    exchange_rate         numeric,
    broker_id             uuid                           not null
        constraint transaction_broker_broker_id_fk
            references finance.broker
            on delete cascade,
    note                  varchar
);

create unique index if not exists transaction_broker_transaction_id_broker_uindex
    on finance.transaction (broker_transaction_id, broker_id);