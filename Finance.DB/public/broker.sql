create table if not exists broker
(
    broker_id          serial
        constraint broker_pk
            primary key,
    name               varchar not null,
    external_broker_id uuid    not null
);

create unique index if not exists broker_broker_id_uindex
    on broker (broker_id);

create unique index if not exists broker_external_broker_id_uindex
    on broker (external_broker_id);

