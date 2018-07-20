-- This file is a part of Music Turret project.
-- 
-- Copyright (C) 2018-2021 Emzi0767
-- 
-- This program is free software: you can redistribute it and/or modify
-- it under the terms of the GNU Affero General Public License as published by
-- the Free Software Foundation, either version 3 of the License, or
-- (at your option) any later version.
-- 
-- This program is distributed in the hope that it will be useful,
-- but WITHOUT ANY WARRANTY; without even the implied warranty of
-- MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
-- GNU Affero General Public License for more details.
-- 
-- You should have received a copy of the GNU Affero General Public License
-- along with this program.  If not, see <https://www.gnu.org/licenses/>.
-- 
-- ----------------------------------------------------------------------------
-- 
-- Turret's PostgreSQL database schema
-- 
-- Version:                  1
-- Bot version:              v1.0.0
-- Timestamp:                2018-07-20 08:24 +02:00
-- Author:                   Emzi0767
-- Project:                  Music Turret
-- License:                  Apache License 2.0
-- PostgreSQL version:       9.6 or above
-- 
-- ----------------------------------------------------------------------------
-- 
-- Types

-- entity_type
-- Determines entity type of the attached ID.
create type entity_kind as enum('user', 'channel', 'guild');

-- ----------------------------------------------------------------------------
-- 
-- Tables

-- metadata
-- This table holds a key-value pairs, which hold various metadata about the 
-- database schema. This table is pre-populated.
create table metadata(
  meta_key text not null,
  meta_value text not null,
  primary key(meta_key)
);
insert into metadata(meta_key, meta_value) values
  ('schema_version', '1'),
  ('timestamp', '2018-07-20T08:24+02:00'),
  ('author', 'Emzi0767'),
  ('project', 'Music Turret'),
  ('license', 'Apache License 2.0');

-- prefixes
-- Holds information about prefixes set in various guilds and channels.
create table prefixes(
  guild_id bigint, -- snowflake
  prefixes text[] not null,
  enable_default boolean not null default true,
  primary key(guild_id)
);

-- blocked_entities
-- Holds information about blocked users, channels, and guilds, along with 
-- information about block reason.
create table blocked_entities(
  id bigint not null, -- snowflake
  kind entity_kind not null,
  reason text,
  since timestamp with time zone not null,
  primary key(id, kind)
);
