CREATE SCHEMA if not exists finance;

create table if not exists finance.user
(
    user_id  uuid default gen_random_uuid(),
    username varchar not null,
    password varchar not null
);

create unique index if not exists user_user_id_uindex
    on finance.user (user_id);

create unique index if not exists user_username_uindex
    on finance.user (username);

