Database structure 


CREATE DATABASE ApplicationTextDB;

USE ApplicationTextDB;

-- 1. Admins Table
CREATE TABLE Admins (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(100) NOT NULL UNIQUE,
    Email VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL
);

-- 2. Users Table
CREATE TABLE Users (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Username VARCHAR(100) NOT NULL UNIQUE,
    Email VARCHAR(255) NOT NULL UNIQUE,
    PasswordHash VARCHAR(255) NOT NULL
);

-- 3. Courses Table (Pre-populated)
CREATE TABLE Courses (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Name VARCHAR(100) NOT NULL UNIQUE
);

-- 4. UserCourses Table (Many-to-Many relationship)
CREATE TABLE UserCourses (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UserId INT NOT NULL,
    CourseId INT NOT NULL,
    FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
    FOREIGN KEY (CourseId) REFERENCES Courses(Id) ON DELETE CASCADE
);

INSERT INTO Courses (Name) VALUES 
('Computer Science'),
('Law'),
('Engineering'),
('Business Management'),
('Psychology'),
('Education'),
('Political Science'),
('Graphic Design'),
('Information Systems'),
('Economics');

SELECT * FROM Courses;

SELECT * FROM Users;

SELECT * FROM Admins;

SHOW TABLES;
DESCRIBE Users;
DESCRIBE Admins;
