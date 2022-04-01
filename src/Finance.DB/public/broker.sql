create table if not exists finance.broker
(
    broker_id  uuid default gen_random_uuid() not null
        constraint broker_pk
            primary key,
    name       varchar                        not null,
    country_id integer                        not null
);
