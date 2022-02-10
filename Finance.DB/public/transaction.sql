create table if not exists transaction
(
    transaction_id          serial
        constraint transaction_pk
            primary key,
    external_transaction_id uuid                     not null,
    broker_transaction_id   varchar,
    ticker_id               integer                  not null
        constraint transaction_ticker_ticker_id_fk
            references ticker
            on delete cascade,
    date                    timestamp with time zone not null,
    units                   numeric                  not null,
    price                   numeric                  not null,
    local_price             numeric,
    fee                     numeric                  not null,
    exchange_rate           numeric,
    broker_id               integer                  not null
        constraint transaction_broker_broker_id_fk
            references broker
            on delete cascade,
    note                    varchar
);

create unique index if not exists transaction_broker_transaction_id_broker_uindex
    on transaction (broker_transaction_id, broker_id);

create unique index if not exists transaction_external_transaction_id_uindex
    on transaction (external_transaction_id);


d);

