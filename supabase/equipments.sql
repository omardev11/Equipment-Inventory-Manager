-- Equipment Inventory Manager - Supabase schema
-- Anonymous users can read inventory for the static viewer.
-- Writes are intentionally denied until you add an authenticated policy or API.

create extension if not exists pgcrypto;

create table if not exists public.equipments (
  id uuid primary key,
  sequence_no integer not null,
  group_name text not null default '',
  equipment_name text not null default '',
  unit text not null default 'Adet',
  quantity integer not null default 1,
  model text not null default '',
  serial_no text not null default '',
  location text not null default '',
  owner text not null default '',
  synced_at timestamptz not null default now(),
  updated_at timestamptz not null default now()
);

create index if not exists idx_equipments_sequence_no on public.equipments(sequence_no);
create index if not exists idx_equipments_group_name on public.equipments(group_name);
create index if not exists idx_equipments_location on public.equipments(location);
create index if not exists idx_equipments_owner on public.equipments(owner);
create index if not exists idx_equipments_serial_no on public.equipments(serial_no);

create or replace function public.set_updated_at()
returns trigger
language plpgsql
as $$
begin
  new.updated_at = now();
  return new;
end;
$$;

drop trigger if exists trg_equipments_updated_at on public.equipments;
create trigger trg_equipments_updated_at
before update on public.equipments
for each row
execute function public.set_updated_at();

alter table public.equipments enable row level security;

drop policy if exists "equipments_select_all" on public.equipments;
create policy "equipments_select_all"
on public.equipments
for select
to anon, authenticated
using (true);

-- Remove the insecure prototype policy if it exists from an older deployment.
drop policy if exists "equipments_write_all" on public.equipments;

-- Add a narrowly scoped policy for authenticated users, or perform writes through
-- a trusted API/Edge Function. Never grant unrestricted writes to the anon role.
