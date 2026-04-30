namespace LibraryApp.DataAccess
{
    public static class Queries
    {
        // Книги
        public const string Books_All = @"
            SELECT b.Id, b.Title, a.Name AS AuthorName, b.Publisher, b.Genre, b.Year, b.IsAvailable
            FROM Books b
            JOIN Authors a ON b.AuthorId = a.Id";

        public const string Books_WithSearch = @"
            SELECT b.Id, b.Title, a.Name AS AuthorName, b.Publisher, b.Genre, b.Year, b.IsAvailable
            FROM Books b
            JOIN Authors a ON b.AuthorId = a.Id
            WHERE b.Title LIKE @keyword
                OR a.Name LIKE @keyword
                OR b.Genre LIKE @keyword
                OR b.Publisher LIKE @keyword";

        public const string Books_CheckBeforeDelete = "SELECT COUNT(*) FROM BorrowedBooks WHERE BookId = @Id";
        public const string Books_Delete = "DELETE FROM Books WHERE Id=@Id";

        // Выданные книги
        public const string Borrowed_Active = @"
            SELECT bb.Id,
                b.Title AS BookTitle,
                r.FullName AS ReaderName,
                bb.BorrowDate,
                bb.ExpectedReturnDate,
                CASE 
                    WHEN bb.ExpectedReturnDate < GETDATE() AND bb.ReturnDate IS NULL 
                    THEN 1 ELSE 0 
                END AS IsOverdue
            FROM BorrowedBooks bb
            JOIN Books b ON bb.BookId = b.Id
            JOIN Readers r ON bb.ReaderId = r.Id
            WHERE bb.ReturnDate IS NULL";

        public const string Borrowed_WithSearch = @"
            SELECT 
                bb.Id,
                b.Title AS BookTitle,
                r.FullName AS ReaderName,
                bb.BorrowDate,
                bb.ExpectedReturnDate,
                CASE WHEN bb.ExpectedReturnDate < GETDATE() THEN 1 ELSE 0 END AS IsOverdue
            FROM BorrowedBooks bb
            JOIN Books b ON bb.BookId = b.Id
            JOIN Readers r ON bb.ReaderId = r.Id
            WHERE bb.ReturnDate IS NULL
            AND (
                b.Title LIKE @keyword
                OR r.FullName LIKE @keyword
            )";

        // Авторы
        public const string Authors_All = "SELECT Id, Name, DateOfBirth FROM Authors";
        public const string Authors_WithSearch = @"
            SELECT Id, Name, DateOfBirth
            FROM Authors
            WHERE Name LIKE @keyword";
        public const string Authors_CheckBeforeDelete = "SELECT COUNT(*) FROM Books WHERE AuthorId = @AuthorId";
        public const string Authors_Delete = "DELETE FROM Authors WHERE Id=@Id";

        // Читатели
        public const string Readers_All = @"
            SELECT Id, FullName, DateOfBirth, PhoneNumber, Email, Password,
                   CASE Role WHEN 1 THEN 'Admin' ELSE 'User' END AS RoleName
            FROM Readers";

        public const string Readers_WithSearch = @"
            SELECT Id, FullName, DateOfBirth, PhoneNumber, Email, Password,
                CASE Role WHEN 1 THEN 'Admin' ELSE 'User' END AS RoleName
            FROM Readers
            WHERE FullName LIKE @keyword
            OR PhoneNumber LIKE @keyword
            OR Email LIKE @keyword";

        public const string Readers_CheckBeforeDelete = "SELECT COUNT(*) FROM BorrowedBooks WHERE ReaderId = @ReaderId";
        public const string Readers_Delete = "DELETE FROM Readers WHERE Id=@Id";
        public const string Readers_GetInfo = "SELECT DateOfBirth, PhoneNumber, Email FROM Readers WHERE FullName = @fullName";

        // История
        public const string History_All = @"
            SELECT 
                b.Title AS BookTitle,
                r.FullName AS ReaderName,
                bb.BorrowDate,
                bb.ExpectedReturnDate,
                bb.ReturnDate
            FROM BorrowedBooks bb
            JOIN Books b ON bb.BookId = b.Id
            JOIN Readers r ON bb.ReaderId = r.Id
            WHERE bb.ReturnDate IS NOT NULL
            ORDER BY bb.ReturnDate DESC";

        public const string History_WithSearch = @"
            SELECT 
                b.Title AS BookTitle,
                r.FullName AS ReaderName,
                bb.BorrowDate,
                bb.ExpectedReturnDate,
                bb.ReturnDate
            FROM BorrowedBooks bb
            JOIN Books b ON bb.BookId = b.Id
            JOIN Readers r ON bb.ReaderId = r.Id
            WHERE bb.ReturnDate IS NOT NULL
            AND (
                b.Title LIKE @keyword
                OR r.FullName LIKE @keyword
            )
            ORDER BY bb.ReturnDate DESC";

        // Статистика
        public const string Stats_PopularBooks = @"
            SELECT TOP 10
                ROW_NUMBER() OVER (ORDER BY COUNT(*) DESC) AS Rank,
                B.Title,
                A.Name AS AuthorName,
                COUNT(*) AS BorrowCount,
                B.Genre
            FROM BorrowedBooks BB
            JOIN Books B ON BB.BookId = B.Id
            JOIN Authors A ON B.AuthorId = A.Id
            GROUP BY B.Title, A.Id, A.Name, B.Genre  -- ← Убрали B.Id, добавили A.Id для надёжности
            ORDER BY BorrowCount DESC";

        public const string Stats_PopularAuthors = @"
            SELECT TOP 10
                ROW_NUMBER() OVER (ORDER BY COUNT(*) DESC) AS Rank,
                A.Name,
                COUNT(*) AS TotalBorrows,
                COUNT(DISTINCT B.Id) AS UniqueBooks,
                A.DateOfBirth
            FROM BorrowedBooks BB
            JOIN Books B ON BB.BookId = B.Id
            JOIN Authors A ON B.AuthorId = A.Id
            GROUP BY A.Id, A.Name, A.DateOfBirth
            ORDER BY TotalBorrows DESC";

        // ПОЛЬЗОВАТЕЛЬСКИЙ ИНТЕРФЕЙС (UserMainWindow)

        // Книги для пользователя (каталог)
        public const string UserBooks_All = @"
            SELECT b.Title, a.Name AS AuthorName, b.Publisher, b.Genre, b.Year, 
                   CASE WHEN b.IsAvailable=1 THEN 'Доступна' ELSE 'Выдана' END AS IsAvailable 
            FROM Books b 
            JOIN Authors a ON b.AuthorId = a.Id";

        public const string UserBooks_WithSearch = @"
            SELECT b.Title, a.Name AS AuthorName, b.Publisher, b.Genre, b.Year, 
                   CASE WHEN b.IsAvailable=1 THEN 'Доступна' ELSE 'Выдана' END AS IsAvailable 
            FROM Books b 
            JOIN Authors a ON b.AuthorId = a.Id
            WHERE b.Title LIKE @search OR a.Name LIKE @search OR b.Publisher LIKE @search OR b.Genre LIKE @search";

        // Мои книги (выданные текущему пользователю)
        public const string UserBorrowed_Mine = @"
            SELECT b.Title AS BookTitle, 
                   a.Name AS AuthorName, 
                   b.Publisher, 
                   bb.BorrowDate, 
                   bb.ExpectedReturnDate,
                   bb.ReturnDate,
                   CASE WHEN bb.ExpectedReturnDate < GETDATE() AND bb.ReturnDate IS NULL THEN 1 ELSE 0 END AS IsOverdue
            FROM BorrowedBooks bb
            JOIN Books b ON bb.BookId = b.Id
            JOIN Authors a ON b.AuthorId = a.Id
            WHERE bb.ReaderId = @userId";

        public const string UserBorrowed_MineWithSearch = @"
            SELECT b.Title AS BookTitle, 
                   a.Name AS AuthorName, 
                   b.Publisher, 
                   bb.BorrowDate, 
                   bb.ExpectedReturnDate,
                   bb.ReturnDate,
                   CASE WHEN bb.ExpectedReturnDate < GETDATE() AND bb.ReturnDate IS NULL THEN 1 ELSE 0 END AS IsOverdue
            FROM BorrowedBooks bb
            JOIN Books b ON bb.BookId = b.Id
            JOIN Authors a ON b.AuthorId = a.Id
            WHERE bb.ReaderId = @userId
            AND (b.Title LIKE @search OR a.Name LIKE @search OR b.Publisher LIKE @search)";
    }
}