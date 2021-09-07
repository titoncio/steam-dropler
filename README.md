# Steam-dropper: The easiest way to farm steam items

Based on https://github.com/kokole/SteamItemDropIdler

Libraries used:
https://github.com/SteamRE/SteamKit
https://github.com/Jessecar96/SteamDesktopAuthenticator

## Quick use

Create a file at *\steam-dropper\Configs\Accounts* using the following fields and save with the account login as the filename and json as extension:

Field | Type | Required
------|------|---------
Password | string | true
SteamId | long | false
IdleEnable | bool | true*
LastRun | string | false
IdleNow | bool | false
LoginKey | string | false
SentryHash | string | false
SharedSecret | string | false
DropConfig | [int/long] | true
TimeConfig | [int/int] | true

*If the value is false the bot won't farm items

Create a file at *\steam-dropper\Configs\maFiles* using the following format and sabe with the same name as the previous config and maFile as extension:

Field | Type | Required
------|------|---------
shared_secret | string | true
serial_number | string | false
revocation_code | string | false
uri | string | false
server_time | int | false
account_name | string | false
token_gid | string | false
identity_secret | string | true
secret_1 | string | false
status | int | true
device_id | string | true


# Translation needed

## Как работает

1. Бот каждые 30 секунд проверяет возможность запустить на идлинг новый аккаунт 
1. Если количество работающих аккаунтов меньше числа parallelCount, то выбирается кандидат в  соответсвии с его расписанием. На данный момент расписание самое простое (1 час идлинга на каждые 12 часов), можно поменять в конфигах
1. В итоге кандидат выбирается так:
   1. Если флаг IdleEnable выставлен в True
   1. И если аккаунт не идлился последние 12 часов

По итогу за неделю каждый аккаунт наигрывает для каждой игры по 14 часов

Во время работы в файл конфига пишется информация о последнем запуске. Это стоит учитывать, т.к. если вы будете закрывать бота в тот момент пока аккаунты идлятся, он запустится на снова на еще 1 час.
  
## Фишки
- Все игры на аккаунте запускаются одновременно (до 32 штук)
- Дроп проверяется перед началом фарма и каждые пол часа во время фарма (что бы уменьшит количество запросов к серверу).
- изменен подход к подключению к серверам Steam
  - в версии kokole использовался steam.dll и подключение осуществлялось случайно
  - в текущей версии напрямую выбирается сервера из ~200 серверов 
  - на каждый сервер подключаются до 12 (возможно увеличить) аккаунтов
  - так что одновременное количество ботов может быть 2400 (не проверялось)
  
 ## Фишки уже существующие, осталось впилить
 - [ ] Настройка семейного доступа
 - [ ] Получение списка игр для аккаунта (из сети, а не из конфига)
 - [ ] Получение кода авторизации через почту
 - [ ] Ввод кода вручную (нужно впилить GUI)
 
 ## Фишки, которые надо продумать
 - [ ] Настройка расписания для каджой игры, с возможностью редактирования (в связи с эвентами KF2 например)
 - [ ] Расчет по конфигам расписания оптимального запуска ботов
 - [ ] GUI для отслеживания состояния ботов (истории, ошибок) и взаимодействия с пользователем
 - [ ] Покупка игры, принятие кода, покупка игры в падарок 
 - [ ] Автоматическое создание ботов (почта->мобильный аутентификатор)
 - [ ] Перенос функционала ArchiSteamFarm для передачи шмоток 
 - [ ] Дроп TF2


  
