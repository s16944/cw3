PromoteStudents 'IT', 1

ALTER PROCEDURE PromoteStudents @Studies NVARCHAR(100), @Semester INT
    AS
    BEGIN

        SET XACT_ABORT ON;
        BEGIN TRANSACTION;

        DECLARE @IdStudies INT = (SELECT IdStudy FROM Studies WHERE Name = @Studies)
        IF @IdStudies IS NULL
            BEGIN
                THROW 50001, 'No such studies', 0;
            END

        DECLARE @IdEnrollment INT = (
            SELECT IdEnrollment
            FROM Enrollment
            WHERE Semester = @Semester + 1
              AND IdStudy = @IdStudies
        )

        IF NOT EXISTS(
                SELECT 1
                FROM Enrollment
                WHERE IdStudy = @IdStudies
                  AND Semester = @Semester
            )
            BEGIN
                THROW 50002, 'No such studies', 0;
            END

        IF @IdEnrollment IS NULL
            BEGIN
                DECLARE @IdEnrollmentTab table(idEnrollment Int NOT NULL);

                INSERT INTO Enrollment (IdEnrollment, Semester, IdStudy, StartDate)
                OUTPUT inserted.IdEnrollment INTO @IdEnrollmentTab
                SELECT MAX(IdEnrollment) + 1, @Semester + 1, @IdStudies, GETDATE()
                FROM Enrollment;

                SELECT @IdEnrollment = idEnrollment FROM @IdEnrollmentTab;
            END

        DECLARE @OldEnrollment INT = (
            SELECT IdEnrollment
            FROM Enrollment
            WHERE Semester = @Semester
              AND IdStudy = @IdStudies
        )

        UPDATE Student
        SET IdEnrollment = @IdEnrollment
        WHERE IdEnrollment = @OldEnrollment;
            
        COMMIT;

        SELECT IdEnrollment, Semester, E.IdStudy, StartDate, Name
        FROM Enrollment E
                 INNER JOIN Studies S on E.IdStudy = S.IdStudy
        WHERE E.IdStudy = @IdStudies AND Semester = @Semester + 1;
        RETURN;
    END;