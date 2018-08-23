using RikardLib.AspLog;
using RikardWeb.Lib.Db.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using RikardLib.Parallel;
using RikardWeb.Lib.Identity.Data;

namespace RikardWeb.Lib.Identity
{
    public class UsersService<T> : IUsersService<T> where T : IdentityUser
    {
        private const string USERS_COLLECTION_NAME = "users";

        private readonly IAspLogger logger;
        private readonly IMongoCollection<T> users;
        private readonly ParallelGatherSingle<ProlongationPayment> worker;

        public UsersService(IAspLogger logger, IMongoDbService mongoDbService)
        {
            this.logger = logger;

            users = mongoDbService.GetDatabase().MongoDatabase.GetCollection<T>(USERS_COLLECTION_NAME);

            worker = new ParallelGatherSingle<ProlongationPayment>(DoProlongationPayment);

            logger.Info("UsersService has been initialized.");
        }

        public T GetUserByIdSync(string userId)
        {
            if(string.IsNullOrWhiteSpace(userId))
            {
                return null;
            }
            return users.Find(_ => _.Id == userId).FirstOrDefault();
        }

        private async Task DoProlongationPaymentAsync(ProlongationPayment payment)
        {
            var user = await users.Find(_ => _.Id == payment.UserId).FirstOrDefaultAsync();

            if (user != null)
            {
                TimeSpan? period = null;

                switch(payment.Type.ToLower())
                {
                    case "hour":
                        period = new TimeSpan(1, 5, 0);
                        break;
                    case "day":
                        period = new TimeSpan(1, 1, 0, 0);
                        break;
                    case "week":
                        period = new TimeSpan(7, 1, 0, 0);
                        break;
                    case "month":
                        period = new TimeSpan(32, 0, 0, 0);
                        break;
                }

                if (period.HasValue)
                {
                    if(user.Balance == null)
                    {
                        user.Balance = new UserBalance();
                    }

                    if(user.Balance.Pays == null)
                    {
                        user.Balance.Pays = new List<UserPay>();
                    }

                    if(user.Balance.ServiceExpired < DateTime.Now)
                    {
                        user.Balance.ServiceExpired = DateTime.Now;
                    }

                    user.Balance.ServiceExpired = user.Balance.ServiceExpired.Add(period.Value);
                    user.Balance.Pays.Add(new UserPay { Amount = payment.Amount, Type = payment.Type, Description = payment.Description });

                    var filter = Builders<T>.Filter.Eq(_ => _.Id, user.Id);
                    var update = Builders<T>.Update.Set(_ => _.Balance, user.Balance);
                    await users.UpdateOneAsync(filter, update);

                    logger.Info($"UsersService: added payment for {user.Id} with amount {payment.Amount} and type {payment.Type}");
                }
                else
                {
                    logger.Error($"UsersService: unknown type {payment.Amount} on payment");
                }
            }
            else
            {
                logger.Error($"UsersService: unknown userid {payment.UserId} on payment");
            }
        }

        private void DoProlongationPayment(ProlongationPayment payment)
        {
            Task.Run(async () => await DoProlongationPaymentAsync(payment)).GetAwaiter().GetResult();
        }

        public void AddProlongationPayment(string userId, double amount, string type, string description)
        {
            worker.AddData(new ProlongationPayment { UserId = userId, Amount = amount, Type = type, Description = description });
        }

        public async Task<bool> IsPhoneNumberExists(string phoneNumber, string userId = null)
        {
            var nPhoneNumber = NormalizePhone(phoneNumber);
            using (var cursor = await users.FindAsync(new BsonDocument()))
            {
                while (await cursor.MoveNextAsync())
                {
                    foreach (var u in cursor.Current)
                    {
                        if(NormalizePhone(u.PhoneNumber) == nPhoneNumber)
                        {
                            if(userId != null)
                            {
                                if(u.Id != userId)
                                {
                                    return true;
                                }
                            }
                            else
                            {
                                return true;
                            }
                        }
                    }
                }
            }
            return false;
        }

        private string NormalizePhone(string p)
            => new string(p.Where(char.IsDigit).ToArray());

        private class ProlongationPayment
        {
            public string UserId { get; internal set; }
            public double Amount { get; internal set; }
            public string Type { get; internal set; }
            public string Description { get; internal set; }
        }
    }
}
