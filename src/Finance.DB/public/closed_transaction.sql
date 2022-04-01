create table if not exists finance.closed_transaction
(
    buy_transaction_id  uuid not null
        constraint transaction_transaction_transaction_transaction_id_fk
            references finance.transaction
            on delete cascade,
    sell_transaction_id uuid not null
        constraint transaction_transaction_transaction_transaction_id_fk_2
            references finance.transaction,
    units               numeric,
    constraint transaction_transaction_pk
        primary key (buy_transaction_id, sell_transaction_id)
);

