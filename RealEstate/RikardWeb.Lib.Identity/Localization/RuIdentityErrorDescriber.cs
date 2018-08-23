using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RikardWeb.Lib.Identity.Localization
{
    public class RuIdentityErrorDescriber : IdentityErrorDescriber
    {
        public override IdentityError DefaultError()
        {
            return new IdentityError()
            {
                Code = "DefaultError",
                Description = "Возникла неизвестная ошибка."
            };
        }

        public override IdentityError ConcurrencyFailure()
        {
            return new IdentityError()
            {
                Code = "ConcurrencyFailure",
                Description = "Ошибка одновременного доступа, объект был модифицирован."
            };
        }

        public override IdentityError PasswordMismatch()
        {
            return new IdentityError()
            {
                Code = "PasswordMismatch",
                Description = "Неверный пароль."
            };
        }

        public override IdentityError InvalidToken()
        {
            return new IdentityError()
            {
                Code = "InvalidToken",
                Description = "Неверный токен."
            };
        }

        public override IdentityError LoginAlreadyAssociated()
        {
            return new IdentityError()
            {
                Code = "LoginAlreadyAssociated",
                Description = "Пользователь с таким же логином уже есть в системе."
            };
        }

        public override IdentityError InvalidUserName(string userName)
        {
            IdentityError identityError = new IdentityError();
            identityError.Code = "InvalidUserName";
            string str = $"Нерное имя '{userName}', имя может содержать только буквы и цифры.";
            identityError.Description = str;
            return identityError;
        }

        public override IdentityError InvalidEmail(string email)
        {
            IdentityError identityError = new IdentityError();
            identityError.Code = "InvalidEmail";
            string str = $"Неправильный '{email}'.";
            identityError.Description = str;
            return identityError;
        }

        public override IdentityError DuplicateUserName(string userName)
        {
            IdentityError identityError = new IdentityError();
            identityError.Code = "DuplicateUserName";
            string str = $"Пользователь с таким же именем '{userName}' уже есть в системе.";
            identityError.Description = str;
            return identityError;
        }

        public override IdentityError DuplicateEmail(string email)
        {
            IdentityError identityError = new IdentityError();
            identityError.Code = "DuplicateEmail";
            string str = $"Пользователь с таким же e-mail '{email}' уже есть в системе.";
            identityError.Description = str;
            return identityError;
        }

        public override IdentityError InvalidRoleName(string role)
        {
            IdentityError identityError = new IdentityError();
            identityError.Code = "InvalidRoleName";
            string str = $"Неверное имя роли '{role}'.";
            identityError.Description = str;
            return identityError;
        }

        public override IdentityError DuplicateRoleName(string role)
        {
            IdentityError identityError = new IdentityError();
            identityError.Code = "DuplicateRoleName";
            string str = $"Такая же роль '{role}' уже есть в списке ролей";
            identityError.Description = str;
            return identityError;
        }

        public override IdentityError UserAlreadyHasPassword()
        {
            return new IdentityError()
            {
                Code = "UserAlreadyHasPassword",
                Description = "У пользователя уже есть пароль."
            };
        }

        public override IdentityError UserLockoutNotEnabled()
        {
            return new IdentityError()
            {
                Code = "UserLockoutNotEnabled",
                Description = "Блокировка для этого пользователя не включена."
            };
        }

        public override IdentityError UserAlreadyInRole(string role)
        {
            IdentityError identityError = new IdentityError();
            identityError.Code = "UserAlreadyInRole";
            string str = $"Пользователь уже имеет эту роль '{role}'.";
            identityError.Description = str;
            return identityError;
        }

        public override IdentityError UserNotInRole(string role)
        {
            IdentityError identityError = new IdentityError();
            identityError.Code = "UserNotInRole";
            string str = $"Пользователь не имеет этой роли '{role}'.";
            identityError.Description = str;
            return identityError;
        }

        public override IdentityError PasswordTooShort(int length)
        {
            IdentityError identityError = new IdentityError();
            identityError.Code = "PasswordTooShort";
            string str = $"Пароль должен быть длиной не менее {length} символов.";
            identityError.Description = str;
            return identityError;
        }

        public override IdentityError PasswordRequiresNonAlphanumeric()
        {
            return new IdentityError()
            {
                Code = "PasswordRequiresNonAlphanumeric",
                Description = "Пароль должен содержать по крайней мере один любой НЕ алфавитно-цифровой символ."
            };
        }

        public override IdentityError PasswordRequiresDigit()
        {
            return new IdentityError()
            {
                Code = "PasswordRequiresDigit",
                Description = "Пароль должен содержать по крайней мере одну цифру."
            };
        }

        public override IdentityError PasswordRequiresLower()
        {
            return new IdentityError()
            {
                Code = "PasswordRequiresLower",
                Description = "Пароль должен содержать по крайней мере одну строчную букву."
            };
        }

        public override IdentityError PasswordRequiresUpper()
        {
            return new IdentityError()
            {
                Code = "PasswordRequiresUpper",
                Description = "Пароль по крайней мере должен содержать по крайней мере одну прописную букву."
            };
        }
    }
}
