-- Tạo user
CREATE USER vguser WITH PASSWORD 'U1b01LwWsj67FFqRjEv0Q95K';

-- Gán owner cho database
ALTER DATABASE vaultguard OWNER TO vguser;

-- Thiết lập schema
ALTER SCHEMA public OWNER TO vguser;
GRANT ALL ON SCHEMA public TO vguser;

-- Thiết lập default privileges
ALTER DEFAULT PRIVILEGES FOR USER vguser IN SCHEMA public 
GRANT ALL ON TABLES TO vguser;

ALTER DEFAULT PRIVILEGES FOR USER vguser IN SCHEMA public 
GRANT ALL ON SEQUENCES TO vguser;

ALTER DEFAULT PRIVILEGES FOR USER vguser IN SCHEMA public 
GRANT ALL ON FUNCTIONS TO vguser;

-- Revoke quyền từ public (tuỳ chọn)
REVOKE CREATE ON SCHEMA public FROM PUBLIC;
REVOKE ALL ON DATABASE vaultguard FROM PUBLIC;