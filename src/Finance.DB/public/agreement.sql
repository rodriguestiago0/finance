create table if not exists finance.greement
(
    agreement_id          uuid                     not null
        constraint agreement_pk
            primary key,
    created_at            timestamp with time zone not null,
    max_historical_days   integer                  not null,
    access_valid_for_days integer                  not null,
    access_scope          varchar                  not null,
    accepted              boolean,
    institution_id        varchar                  not null
);

