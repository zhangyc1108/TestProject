using Common.ORM;
using System;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.Text;

namespace Common.DB
{
    [Serializable]
    public class AccountDB : DBEntity
    {
        [BsonConstructor]
        public AccountDB(string Account, string MD5str)
        {
            this.Account = Account;

            this.MD5str = MD5str;
        }

        /// <summary>
        /// 账号名
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 账号名和密码生成的MD5码
        /// </summary>
        public string MD5str { get; set; }
    }
}