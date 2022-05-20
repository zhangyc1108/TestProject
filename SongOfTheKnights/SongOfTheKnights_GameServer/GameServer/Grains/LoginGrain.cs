﻿using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Common;
using Common.DB;
using Common.ORM;
using Google.Protobuf;
using IGrains;
using LaunchPB;

namespace IGrains
{
    class LoginGrain : Orleans.Grain, ILoginGrain
    {
        /// <summary>
        /// 账号表
        /// </summary>
        private Table<AccountDB> accountTable = new Table<AccountDB>();

        /// <summary>
        /// 角色表
        /// </summary>
        private Table<UserDB> userTable = new Table<UserDB>();

        /// <summary>
        /// 网关服务器的账号登录请求
        /// </summary>
        /// <param name="netPackage"></param>
        /// <returns></returns>
        public Task<NetPackage> OnLogin(NetPackage netPackage)
        {
            IMessage message = new Login();

            Login login = message.Descriptor.Parser.ParseFrom(netPackage.bodyData, 0, netPackage.bodyData.Length) as Login;

            // 账号或者密码为空

            if (string.IsNullOrEmpty(login.Account) || string.IsNullOrEmpty(login.Password))
            {
                return Task.FromResult(new NetPackage()
                {
                    protoID = (int)ProtoCode.ELoginResp,

                    bodyData = new LoginResp() { Result = LoginResult.EInputNull }.ToByteArray()
                });
            }

            // 账号不存在

            List<AccountDB> list = accountTable.Find(t => t.Account.Equals(login.Account));

            if (list == null || list.Count == 0)
            {
                return Task.FromResult(new NetPackage()
                {
                    protoID = (int)ProtoCode.ELoginResp,

                    bodyData = new LoginResp() { Result = LoginResult.EInputWrong }.ToByteArray()
                });
            }

            // 密码错误

            string md5Str = Encoding.UTF8.GetString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(login.Account + login.Password)));

            if (md5Str != list[0].MD5str)
            {
                return Task.FromResult(new NetPackage()
                {
                    protoID = (int)ProtoCode.ELoginResp,

                    bodyData = new LoginResp() { Result = LoginResult.EInputWrong }.ToByteArray()
                });
            }

            // 登录成功消息

            Logger.Instance.Information($"{login.Account} 登录成功");

            return Task.FromResult(new NetPackage()
            {
                protoID = (int)ProtoCode.ELoginResp,

                bodyData = new LoginResp()
                {
                    Result = LoginResult.ELoginSuccess,

                    Account = login.Account,

                    Password = login.Password
                }.ToByteArray()
            });
        }

        /// <summary>
        /// 网关服务器的账户注册请求
        /// </summary>
        /// <param name="netPackage"></param>
        /// <returns></returns>
        public Task<NetPackage> OnRegister(NetPackage netPackage)
        {
            IMessage message = new Register();

            Register register = message.Descriptor.Parser.ParseFrom(netPackage.bodyData, 0, netPackage.bodyData.Length) as Register;

            // 账号名字不合法

            if (string.IsNullOrEmpty(register.Account) || register.Account.Length > 32)
            {
                NetPackage respPackage = new NetPackage()
                {
                    protoID = (int)ProtoCode.EResgisterResp,

                    bodyData = new RegisterResp() { Result = RegisterResult.EAccountWrong }.ToByteArray()
                };

                return Task.FromResult(respPackage);
            }

            // 账号密码不合法

            if (string.IsNullOrEmpty(register.Password) || register.Password.Length > 32)
            {
                return Task.FromResult(new NetPackage()
                {
                    protoID = (int)ProtoCode.EResgisterResp,

                    bodyData = new RegisterResp() { Result = RegisterResult.EPasswordWrong }.ToByteArray()
                });
            }

            // 账号名字已经被其他人占用

            List<AccountDB> list = accountTable.Find(t => t.Account.Equals(register.Account));

            if (list != null && list.Count > 0)
            {
                return Task.FromResult(new NetPackage()
                {
                    protoID = (int)ProtoCode.EResgisterResp,

                    bodyData = new RegisterResp() { Result = RegisterResult.EAccountExsit }.ToByteArray()
                });
            }

            // 可以注册新账号

            string md5Str = Encoding.UTF8.GetString(MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(register.Account + register.Password)));

            AccountDB accountDB = new AccountDB(register.Account, md5Str);

            accountTable.Add(accountDB);

            UserDB userDB = new UserDB("", 5000);

            userDB.ID = accountDB.ID;

            userDB.Account = accountDB.Account;

            userTable.Add(userDB);

            Logger.Instance.Information($"{accountDB.Account} 注册成功");

            // 注册成功消息

            return Task.FromResult(new NetPackage()
            {
                protoID = (int)ProtoCode.EResgisterResp,

                bodyData = new RegisterResp()
                {
                    Result = RegisterResult.ERegisterSuccess,

                    Account = register.Account,

                    Password = register.Password
                }.ToByteArray()
            });
        }
    }
}