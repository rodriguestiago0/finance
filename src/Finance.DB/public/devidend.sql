create table if not exists finance.devidend
(
    dividend_id uuid default gen_random_uuid() not null
        constraint devidend_pk
            primary key,
    ticker_id   uuid                           not null
        constraint devidend_ticker_ticker_id_fk
            references finance.ticker,
    value       numeric                        not null,
    taxes       numeric,
    received_at timestamp with time zone       not null
);

