-- This file is a part of Music Turret project.
-- 
-- Copyright 2018 Emzi0767
-- 
-- Licensed under the Apache License, Version 2.0 (the "License");
-- you may not use this file except in compliance with the License.
-- You may obtain a copy of the License at
-- 
-- http://www.apache.org/licenses/LICENSE-2.0
-- 
-- Unless required by applicable law or agreed to in writing, software
-- distributed under the License is distributed on an "AS IS" BASIS,
-- WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
-- See the License for the specific language governing permissions and
-- limitations under the License.
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
