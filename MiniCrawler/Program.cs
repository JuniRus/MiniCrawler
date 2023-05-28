/* 
 * MiniCrawler: скелетный вариант поискового робота.
 * Применение: для запуска поискового робота укажите URI
 * в командной строке. Например, для того чтобы начать поиск
 * с адреса www.McGraw-Hill.com, введите следующую команду:
           MiniCrawler.exe http://McGraw-Hill.com
*/

using System;
using System.Net;
using System.IO;
using System.Collections.Generic;
using MiniCrawler;

namespace MiniCrawlerNS
{
    class MiniCrawler
    {
        /// <summary>
        /// Удаляет недействительную позицию по URI.
        /// </summary>
        static void DeleteBadLocation(List<Location> locCurCollect, Location loc)
        {
            foreach (var locCur in locCurCollect)
            {
                if (locCur.Uri == loc.Uri)
                {
                    locCurCollect.Remove(locCur);
                    break;
                }
            }
        }

        /// <summary>
        /// Найти абсолютную ссылку в коде ресурса.
        /// </summary>
        static string FindLinkAbs(string htmlstr,
                               ref int startloc)
        {
            int i;
            int start, end;
            string uri = null;

            i = htmlstr.IndexOf("href=\"http", startloc,
                                StringComparison.OrdinalIgnoreCase);
            if (i != -1)
            {
                // Начальная кавычка ссылки.
                start = htmlstr.IndexOf('"', i) + 1;
                // Конечная кавычка ссылки.
                end = htmlstr.IndexOf('"', start);
                // Сохранить найденую ссылку в возвращаемой переменной.
                uri = htmlstr.Substring(start, end - start);
                // При поиске следующей ссылки пропустить сохранённый uri.
                startloc = end;
            }

            return uri;
        }

        /// <summary>
        /// Найти относительную ссылку в коде ресурса.
        /// </summary>
        static string FindLinkRel(string htmlstr,
                               ref int startloc)
        {
            int i;
            int start, end;
            string uri = null;

            // Находится положение адреса URI в коде ресурса.
            i = htmlstr.IndexOf("href=\"/", startloc,
                                StringComparison.OrdinalIgnoreCase);
            if (i != -1)
            {
                // Начальная кавычка ссылки.
                start = htmlstr.IndexOf('"', i) + 1;
                // Конечная кавычка ссылки.
                end = htmlstr.IndexOf('"', start);
                // Сохранить найденую ссылку в возвращаемой переменной.
                uri = htmlstr.Substring(start, end - start);
                // При поиске следующей ссылки пропустить сохранённый uri.
                startloc = end;
            }

            return uri;
        }

        static void Main(string[] args)
        {
            // Строка, содержащая найденую абсолютную ссылку.
            string linkAbs = null;
            // Строка, содержащая найденую относительную ссылку.
            string linkRel = null;

            // Стек, содержащий адреса URI.
            Stack<string> stackUri = new Stack<string>();
            // Список пустых ссылок URI.
            List<string> uriNull = new List<string>();
            // Список всех URI с позициями.
            List<Location> locCurCollect = new List<Location>();

            // Всё содержимое сайта.
            string htmlStr;
            // Объект запроса.
            HttpWebResponse resp = null;

            // Содержит текущее положение абсолютной ссылки в ответе.
            int curlocAbs;
            // Содержит текущее положение относительной ссылки в ответе.
            int curlocRel;

            // Закомментировать при разработке.
            if (args.Length != 1)
            {
                Console.WriteLine("Применение: MiniCrawler.exe <uri>");
                Console.Read();
                return;
            }

            // Закомментировать при разработке.
            string uriStr = args[0]; // содержит текущий URI. 

            // Раскоментировать при разработке.
            //string uriStr = "https://github.com/";
            string uriPrev = null;

            // Добавить первую ссылку в стек.
            stackUri.Push(uriStr);

            try
            {
                // Извлечение информации из ссылки.
                do
                {
                    // Если URI не содержит ссылок, то присвоить
                    // в него другой адрес из стека.
                    if (uriNull.Contains(uriStr))
                    {
                        if (stackUri.Count > 0)
                        {
                            uriStr = stackUri.Pop();
                        }
                        else
                        {
                            Console.WriteLine("Ссылок больше нет!");
                        }
                    }

                    Console.WriteLine("Переход по ссылке " + uriStr);

                    // Создать объект запроса типа WebRequest по указанному URI.
                    HttpWebRequest req = (HttpWebRequest)
                                          WebRequest.Create(uriStr);

                    uriPrev = uriStr;
                    uriStr = null; // запретить дальнейшее использование этого URI

                    // Отправить сформированный запрос и получить на него ответ.
                    resp = (HttpWebResponse)req.GetResponse();

                    // Получить поток ввода их принятого ответа.
                    Stream istrm = resp.GetResponseStream();

                    // Заключить поток ввода в оболочку класса StreamReader.
                    StreamReader rdr = new StreamReader(istrm);

                    // Прочить всю страницу.
                    htmlStr = rdr.ReadToEnd();

                    curlocAbs = 0;
                    curlocRel = 0;

                    // Если данный адрес URI использовался, 
                    // то извлечь данные о последнем положении ссылок.
                    foreach (var loc in locCurCollect)
                    {
                        if(loc.Uri == uriPrev)
                        {
                            curlocAbs = loc.CurLocAbs;
                            curlocRel = loc.CurLocRel;
                        }
                    }

                    // Нахождение других ссылок.
                    do
                    {
                        // Найти следующий URI для перехода по ссылке.
                        linkAbs = FindLinkAbs(htmlStr, ref curlocAbs);
                        linkRel = FindLinkRel(htmlStr, ref curlocRel);

                        if(linkAbs != null)
                        {
                            Console.WriteLine("Найдена абсолюная ссылка: " + linkAbs);
                        }

                        if (linkRel != null)
                        {
                            Console.WriteLine("Найдена относительная ссылка: " + linkRel);
                            Uri uri = new Uri(string.Concat(linkAbs, linkRel));
                            linkRel = uri.ToString();
                        }

                        // Найдены две ссылки.
                        if(linkAbs != null && linkRel != null)
                        {
                            // Выбрать абсолютную, если она расположена раньше.
                            if(curlocAbs < curlocRel)
                            {
                                uriStr = string.Copy(linkAbs);
                                stackUri.Push(uriPrev);
                                Location loc = new Location(uriPrev, curlocAbs, curlocRel);
                                DeleteBadLocation(locCurCollect, loc);
                                locCurCollect.Add(loc);

                                Console.WriteLine();
                                break;
                            }
                            // Иначе выбрать относительную ссылку.
                            else
                            {
                                uriStr = string.Copy(linkRel);
                                stackUri.Push(uriPrev);
                                Location loc = new Location(uriPrev, curlocAbs, curlocRel);
                                DeleteBadLocation(locCurCollect, loc);
                                locCurCollect.Add(loc);

                                Console.WriteLine();
                                break;
                            }
                        }

                        // Если ссылок не нашлось, то отступить на шаг назад.
                        if(linkAbs == null && linkRel == null)
                        {
                            uriNull.Add(uriPrev);

                            if(stackUri.Count > 0)
                            {
                                uriStr = stackUri.Pop();
                                Console.WriteLine();
                                break;
                            }
                            else
                            {
                                Console.WriteLine("Ссылок больше нет!");
                                break;
                            }
                        }

                        // Если нет относительной ссылки, то выбрать абсолютную.
                        if(linkAbs != null && linkRel == null)
                        {
                            uriStr = string.Copy(linkAbs);
                            stackUri.Push(uriPrev);
                            Location loc = new Location(uriPrev, curlocAbs, curlocRel);
                            DeleteBadLocation(locCurCollect, loc);
                            locCurCollect.Add(loc);

                            Console.WriteLine();
                            break;
                        }

                        // Если нет абсолютной ссылки, то выбрать относительную.
                        if (linkRel != null && linkAbs == null)
                        {
                            uriStr = string.Copy(linkRel);
                            stackUri.Push(uriPrev);
                            Location loc = new Location(uriPrev, curlocAbs, curlocRel);
                            DeleteBadLocation(locCurCollect, loc);
                            locCurCollect.Add(loc);

                            Console.WriteLine();
                            break;
                        }
                    } while (linkAbs != null || linkRel != null);

                    // Закрыть ответный поток.
                    if (resp != null) resp.Close();
                } while (uriStr != null);
            }
            catch (WebException exc)
            {
                Console.WriteLine("Сетевая ошибка: " + exc.Message +
                                  "\nКод состояния: " + exc.Status);
            }
            catch (ProtocolViolationException exc)
            {
                Console.WriteLine("Ошибка нарушения протокола: " + exc.Message);
            }
            catch (UriFormatException exc)
            {
                Console.WriteLine("Ошибка формата URI: " + exc.Message);
            }
            catch (NotSupportedException exc)
            {
                Console.WriteLine("Неизвестный протокол: " + exc.Message);
            }
            catch (IOException exc)
            {
                Console.WriteLine("Ошибка ввода-вывода: " + exc.Message);
            }
            finally
            {
                if (resp != null) resp.Close();
            }
            Console.WriteLine("Завершение программы MiniCrawler");
            Console.ReadLine();
        }
    }
}
