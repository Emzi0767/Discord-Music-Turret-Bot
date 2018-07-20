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
-- Version:                  1 to 2
-- Bot version:              v1.2.0
-- Timestamp:                2018-10-01 20:29 +02:00
-- Author:                   Emzi0767
-- Project:                  Music Turret
-- License:                  Apache License 2.0
-- PostgreSQL version:       9.6 or above
-- 
-- ------------------------------------------------------------------------
-- 
-- Tables, migrations, and conversions

-- Update schema version in the database
update metadata set meta_value = '2' where meta_key = 'schema_version';
update metadata set meta_value = '2018-10-01T20:29+02:00' where meta_key = 'timestamp';

-- ------------------------------------------------------------------------

-- blocked_entities -> entity_blacklist
alter table blocked_entities rename to entity_blacklist;
