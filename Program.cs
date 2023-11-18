using Newtonsoft.Json;
using System.IO;
using System.Xml;

namespace PdfOCR
{
    internal class Program
    {
        private enum LineState
        {
            Number,
            Title,
            Author,
            Publisher,
            Year
        }

        private static LineState currentState;

        private static void Main()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "books2.txt");
            string savedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "books.json");

            try
            {
                List<Book> books;

                if (File.Exists(savedPath))
                {
                    books = JsonConvert.DeserializeObject<List<Book>>(File.ReadAllText("books.json"));
                }
                else
                {
                    books = ReadAndParseFile(filePath);
                }

                // 결과 출력
                foreach (var book in books)
                {
                    if (string.IsNullOrEmpty(book.Publisher))
                    {
                        Console.WriteLine($"Book Number: {book.Number}");
                        Console.WriteLine($"Title: {book.Title}");
                        Console.WriteLine($"Author: {book.Author}");
                        Console.WriteLine($"Publisher: Not Available");
                        Console.WriteLine($"Year: {book.Year}");
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.WriteLine(book);
                    }
                }

                while (true)
                {
                    var bookNumbers = books
                        .Where(book => string.IsNullOrEmpty(book.Publisher))
                        .Select(book => book.Number)
                        .ToList();

                    if (bookNumbers.Count == 0)
                    {
                        Console.WriteLine("All books are updated!");
                        break;
                    }

                    foreach (var number in bookNumbers)
                    {
                        Console.WriteLine($"Book Number: {number}");
                    }

                    Console.WriteLine("Enter the book number to modify (0 to exit) :");
                    int bookNumber = int.Parse(Console.ReadLine());

                    // 종료 조건 추가
                    if (bookNumber == 0)
                    {
                        break;
                    }

                    Book selectedBook = books.FirstOrDefault(b => b.Number == bookNumber);

                    if (selectedBook != null)
                    {
                        Console.WriteLine($"Book Number: {selectedBook.Number}");
                        Console.WriteLine($"Title: {selectedBook.Title}");
                        Console.WriteLine($"Author: {selectedBook.Author}");
                        Console.WriteLine($"Publisher: {(string.IsNullOrEmpty(selectedBook.Publisher) ? "Not Available" : selectedBook.Publisher)}");
                        Console.WriteLine($"Year: {selectedBook.Year}");

                        Console.WriteLine($"Enter the new title:");
                        string newTitle = Console.ReadLine();
                        selectedBook.Title = newTitle;

                        Console.WriteLine($"Enter the new author ({selectedBook.Author}):");
                        string newAuthor = Console.ReadLine();
                        selectedBook.Author = newAuthor;

                        Console.WriteLine("Enter the new publisher:");
                        string newPublisher = Console.ReadLine();
                        selectedBook.Publisher = newPublisher;

                        Console.WriteLine("Book information updated successfully!");
                    }
                    else
                    {
                        Console.WriteLine("Book not found!");
                    }
                }

                // Your code here
                // JSON 형식으로 파일에 쓰기
                string json = JsonConvert.SerializeObject(books, Newtonsoft.Json.Formatting.Indented);
                File.WriteAllText("books.json", json);

                // Your code here
                // Markdown 형식으로 파일에 쓰기
                using (StreamWriter writer = new StreamWriter("books.md"))
                {
                    writer.WriteLine("| Book Number | Title | Author | Publisher | Year |");
                    writer.WriteLine("|-------------|-------|--------|-----------|------|");

                    foreach (var book in books)
                    {
                        writer.WriteLine($"| {book.Number} | {book.Title} | {book.Author} | {(string.IsNullOrEmpty(book.Publisher) ? "Not Available" : book.Publisher)} | {book.Year} |");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"currentState: {currentState}");
                Console.WriteLine(ex.Message);
            }
        }

        private static List<Book> ReadAndParseFile(string filePath)
        {
            List<Book> books = new List<Book>();

            // 파일의 모든 줄을 읽어오기
            string[] lines = File.ReadAllLines(filePath);

            // 초기 상태
            currentState = LineState.Number;
            Book currentBook = new Book();

            foreach (var line in lines)
            {
                // 현재 줄이 비어있으면 다음으로 넘어가기
                if (string.IsNullOrWhiteSpace(line) || ShouldIgnore(line))
                {
                    continue;
                }

                switch (currentState)
                {
                    case LineState.Number:
                        if (line.All(char.IsDigit))
                        {
                            currentBook.Number = int.Parse(line);
                            currentState = LineState.Title;
                        }
                        else
                        {
                            string[] dividedText = line.Split(' ');
                            int number = int.Parse(dividedText[0]);
                            string text = string.Join(" ", dividedText.Skip(1));

                            currentBook.Number = number;
                            currentBook.Title = text;
                            currentState = LineState.Author;
                        }
                        break;

                    case LineState.Title:
                        currentBook.Title = line;
                        currentState = LineState.Author;
                        break;

                    case LineState.Author:
                        currentBook.Author = line;
                        currentState = LineState.Publisher;
                        break;

                    case LineState.Publisher:
                        if (line.All(char.IsDigit))
                        {
                            currentBook.Year = int.Parse(line);
                            currentState = LineState.Number;

                            // 한 권의 책이 완성되면 리스트에 추가
                            books.Add(currentBook);
                            currentBook = new Book(); // 새 책을 위해 초기화
                        }
                        else
                        {
                            currentBook.Publisher = line;
                            currentState = LineState.Year;
                        }

                        break;

                    case LineState.Year:
                        currentBook.Year = int.Parse(line);
                        currentState = LineState.Number;

                        // 한 권의 책이 완성되면 리스트에 추가
                        books.Add(currentBook);
                        currentBook = new Book(); // 새 책을 위해 초기화
                        break;
                }
            }

            return books;
        }

        private static bool ShouldIgnore(string line)
        {
            // 무시해야 할 텍스트들을 나타내는 배열
            string[] ignoredTexts = { "번호", "서명", "저자", "출판사", "발행년" };

            // 현재 줄이 무시해야 할 텍스트인지 확인
            return ignoredTexts.Any(ignoredText => line.Equals(ignoredText));
        }
    }
}