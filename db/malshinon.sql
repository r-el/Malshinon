-- DROP DATABASE malshinon
CREATE DATABASE IF NOT EXISTS malshinon;
-- CREATE TABLE IF NOT EXISTS people;
CREATE TABLE IF NOT EXISTS people(
      id INT PRIMARY KEY AUTO_INCREMENT,
      first_name VARCHAR(100),
      last_name VARCHAR(100),
      secret_code VARCHAR(100) UNIQUE,
      type ENUM("reporter", "target", "both", "potential_agent"),
      num_reports INT DEFAULT 0,
      -- used if person is a reporter
      num_mentions INT DEFAULT 0, -- used if person is a target
      CONSTRAINT full_name UNIQUE (first_name, last_name)
);
-- CREATE TABLE IF NOT EXISTS intel_reports;
CREATE TABLE IF NOT EXISTS intel_reports(
      id INT PRIMARY KEY AUTO_INCREMENT,
      reporter_id INT,
      target_id INT,
      text VARCHAR(255),
      timestamp DATETIME DEFAULT CURRENT_TIMESTAMP(),
      FOREIGN KEY(reporter_id) REFERENCES people(id),
      FOREIGN KEY(target_id) REFERENCES people(id)
);