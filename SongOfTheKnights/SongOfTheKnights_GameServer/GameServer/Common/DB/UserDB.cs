using Common.ORM;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Common.DB
{
    [Serializable]
    public class UserDB : DBEntity
    {
        [BsonConstructor]
        public UserDB(string NickName, long Coin)
        {
            this.NickName = NickName;

            this.Coin = Coin;
        }

        /// <summary>
        /// 账号名
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 用户金币数量
        /// </summary>
        public long Coin { get; set; }
    }
}