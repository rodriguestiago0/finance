create table if not exists broker
(
    broker_id  uuid default gen_random_uuid() not null
        constraint broker_pk
            primary key,
    name       varchar                        not null,
    country_id integer                        not null
);

uindex
    on broker (broker_id);
