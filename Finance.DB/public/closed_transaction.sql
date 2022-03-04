create table closed_transaction
(
    buy_transaction_id  integer not null
        constraint transaction_transaction_transaction_transaction_id_fk
            references transaction
            on delete cascade,
    sell_transaction_id integer not null
        constraint transaction_transaction_transaction_transaction_id_fk_2
            references transaction,
    units               numeric,
    constraint transaction_transaction_pk
        primary key (buy_transaction_id, sell_transaction_id)
);

