create table broker
(
    broker_id          serial
        constraint broker_pk
            primary key,
    name               varchar not null,
    external_broker_id uuid    not null
);

create unique index broker_broker_id_uindex
    on broker (broker_id);

create unique index broker_external_broker_id_uindex
    on broker (external_broker_id);

broker_id_uindex
    on broker (external_broker_id);

