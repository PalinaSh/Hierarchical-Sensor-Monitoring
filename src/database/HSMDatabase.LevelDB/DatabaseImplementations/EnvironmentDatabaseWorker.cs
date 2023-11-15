﻿using HSMDatabase.AccessManager;
using HSMDatabase.AccessManager.DatabaseEntities;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HSMDatabase.LevelDB.DatabaseImplementations
{
    internal sealed class EnvironmentDatabaseWorker : IEnvironmentDatabase
    {
        private static readonly JsonSerializerOptions _options = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
            IgnoreReadOnlyProperties = true,
        };

        private readonly byte[] _productListKey = "ProductsNames"u8.ToArray();
        private readonly byte[] _accessKeyListKey = "AccessKeys"u8.ToArray();
        private readonly byte[] _sensorIdsKey = "SensorIds"u8.ToArray();
        private readonly byte[] _policyIdsKey = "NewPolicyIds"u8.ToArray();
        private readonly byte[] _folderIdsKey = "FolderIds"u8.ToArray();
        private readonly byte[] _telegramChatIdsKey = "TelegramChats"u8.ToArray();

        private readonly LevelDBDatabaseAdapter _database;
        private readonly Logger _logger;


        public EnvironmentDatabaseWorker(string name)
        {
            _database = new LevelDBDatabaseAdapter(name);
            _logger = LogManager.GetCurrentClassLogger();
        }


        public void Backup(string backupPath) => _database.Backup(backupPath);


        #region Folders

        public void PutFolder(FolderEntity entity)
        {
            try
            {
                _database.Put(Encoding.UTF8.GetBytes(entity.Id), JsonSerializer.SerializeToUtf8Bytes(entity));
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to put folder info for {entity.Id}");
            }
        }

        public void RemoveFolder(string id)
        {
            try
            {
                _database.Delete(Encoding.UTF8.GetBytes(id));
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to remove info for folder {id}");
            }
        }

        public void AddFolderToList(string id)
        {
            try
            {
                var currentList = GetFoldersList();

                if (!currentList.Contains(id))
                    currentList.Add(id);

                _database.Put(_folderIdsKey, JsonSerializer.SerializeToUtf8Bytes(currentList));
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to add folder {id} to list");
            }
        }

        public void RemoveFolderFromList(string id)
        {
            try
            {
                var currentList = GetFoldersList();

                currentList.Remove(id);

                _database.Put(_folderIdsKey, JsonSerializer.SerializeToUtf8Bytes(currentList));
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to remove folder {id} from list");
            }
        }

        public FolderEntity GetFolder(string id)
        {
            try
            {
                return _database.TryRead(Encoding.UTF8.GetBytes(id), out byte[] value)
                    ? JsonSerializer.Deserialize<FolderEntity>(value)
                    : null;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to read info for folder {id}");
            }

            return null;
        }

        public List<string> GetFoldersList()
        {
            try
            {
                return _database.TryRead(_folderIdsKey, out byte[] value) ?
                    JsonSerializer.Deserialize<List<string>>(value)
                    : new();
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to get folders ids list");
            }

            return new();
        }

        #endregion

        #region Products

        public void AddProductToList(string productId)
        {
            try
            {
                var currentList = _database.TryRead(_productListKey, out var value)
                    ? JsonSerializer.Deserialize<List<string>>(Encoding.UTF8.GetString(value))
                    : new List<string>();

                if (!currentList.Contains(productId))
                    currentList.Add(productId);

                _database.Put(_productListKey, JsonSerializer.SerializeToUtf8Bytes(currentList));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to add product to list");
            }
        }

        public List<string> GetProductsList()
        {
            var result = new List<string>();
            try
            {
                var products = _database.TryRead(_productListKey, out byte[] value) ?
                    JsonSerializer.Deserialize<List<string>>(Encoding.UTF8.GetString(value))
                    : new List<string>();

                result.AddRange(products);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to get products list");
            }

            return result;
        }

        public ProductEntity GetProduct(string id)
        {
            var bytesKey = Encoding.UTF8.GetBytes(id);
            try
            {
                return _database.TryRead(bytesKey, out byte[] value)
                    ? JsonSerializer.Deserialize<ProductEntity>(Encoding.UTF8.GetString(value)) : null;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to read info for product {id}");
            }

            return null;
        }

        public void PutProduct(ProductEntity product)
        {
            var bytesKey = Encoding.UTF8.GetBytes(product.Id);

            try
            {
                _database.Put(bytesKey, JsonSerializer.SerializeToUtf8Bytes(product));
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to put product info for {product.Id}");
            }
        }

        public void RemoveProduct(string id)
        {
            byte[] bytesKey = Encoding.UTF8.GetBytes(id);
            try
            {
                _database.Delete(bytesKey);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to remove info for product {id}");
            }
        }

        public void RemoveProductFromList(string productId)
        {
            try
            {
                var currentList = _database.TryRead(_productListKey, out byte[] value)
                    ? JsonSerializer.Deserialize<List<string>>(Encoding.UTF8.GetString(value))
                    : new List<string>();

                currentList.Remove(productId);

                _database.Put(_productListKey, JsonSerializer.SerializeToUtf8Bytes(currentList));
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to remove product {productId} from list");
            }
        }

        #endregion

        #region AccessKey

        public void AddAccessKeyToList(string id)
        {
            try
            {
                var currentList = GetAccessKeyList();
                if (!currentList.Contains(id))
                    currentList.Add(id);

                _database.Put(_accessKeyListKey, JsonSerializer.SerializeToUtf8Bytes(currentList));
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to add AccessKey {id} to list");
            }
        }

        public List<string> GetAccessKeyList()
        {
            var result = new List<string>();
            try
            {
                var keys = _database.TryRead(_accessKeyListKey, out byte[] value) ?
                    JsonSerializer.Deserialize<List<string>>(Encoding.UTF8.GetString(value))
                    : new List<string>();

                result.AddRange(keys);
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to get AccessKeys list");
            }

            return result;
        }

        public void RemoveAccessKeyFromList(string id)
        {
            try
            {
                var currentList = GetAccessKeyList();
                currentList.Remove(id);

                _database.Put(_accessKeyListKey, JsonSerializer.SerializeToUtf8Bytes(currentList));
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to remove AccessKey {id} from list");
            }
        }

        public void AddAccessKey(AccessKeyEntity entity)
        {
            var bytesKey = Encoding.UTF8.GetBytes(entity.Id);

            try
            {
                _database.Put(bytesKey, JsonSerializer.SerializeToUtf8Bytes(entity));
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to put AccessKey for {entity.Id}");
            }
        }

        public void RemoveAccessKey(string id)
        {
            byte[] bytesKey = Encoding.UTF8.GetBytes(id);
            try
            {
                _database.Delete(bytesKey);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to remove AccessKey by {id}");
            }
        }

        public AccessKeyEntity GetAccessKey(string id)
        {
            var bytesKey = Encoding.UTF8.GetBytes(id);
            try
            {
                return _database.TryRead(bytesKey, out byte[] value)
                    ? JsonSerializer.Deserialize<AccessKeyEntity>(Encoding.UTF8.GetString(value)) : null;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to read AccessKey by {id}");
            }

            return null;
        }

        #endregion

        #region Sensors

        public void AddSensorIdToList(string sensorId)
        {
            void AddSensorIdToListIfNotExist(List<string> sensorIds)
            {
                if (!sensorIds.Contains(sensorId))
                    sensorIds.Add(sensorId);
            }

            UpdateSensorIdsList(AddSensorIdToListIfNotExist, $"Failed to add sensor id {sensorId} to list");
        }

        public void AddSensor(SensorEntity entity)
        {
            var bytesKey = Encoding.UTF8.GetBytes(entity.Id);
            var bytesValue = JsonSerializer.SerializeToUtf8Bytes(entity);

            try
            {
                _database.Put(bytesKey, bytesValue);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to add sensor info for {entity.Id}");
            }
        }

        public void RemoveSensorIdFromList(string sensorId) =>
            UpdateSensorIdsList(sensorIdsList => sensorIdsList.Remove(sensorId),
                                $"Failed to remove sensor id {sensorId} from list");

        public void RemoveSensor(string sensorId)
        {
            byte[] bytesKey = Encoding.UTF8.GetBytes(sensorId);

            try
            {
                _database.Delete(bytesKey);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to remove sensor info for {sensorId}");
            }
        }

        public SensorEntity GetSensorEntity(string sensorId)
        {
            var bytesKey = Encoding.UTF8.GetBytes(sensorId);

            try
            {
                return _database.TryRead(bytesKey, out byte[] value)
                    ? JsonSerializer.Deserialize<SensorEntity>(Encoding.UTF8.GetString(value))
                    : null;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to read info for sensor {sensorId}");
            }

            return null;
        }

        public List<string> GetAllSensorsIds() =>
            GetListOfKeys(_sensorIdsKey, "Failed to get sensors ids list");

        private void UpdateSensorIdsList(Action<List<string>> updateListAction, string errorMessage)
        {
            try
            {
                var sensorIds = GetAllSensorsIds();

                updateListAction?.Invoke(sensorIds);

                _database.Put(_sensorIdsKey, JsonSerializer.SerializeToUtf8Bytes(sensorIds));
            }
            catch (Exception e)
            {
                _logger.Error(e, errorMessage);
            }
        }

        #endregion

        #region Policies

        public void AddPolicyIdToList(Guid policyId)
        {
            try
            {
                var policyIds = GetAllPoliciesIds();

                if (!policyIds.Select(g => new Guid(g)).Contains(policyId))
                    policyIds.Add(policyId.ToByteArray());

                _database.Put(_policyIdsKey, JsonSerializer.SerializeToUtf8Bytes(policyIds));
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to add policy id {policyId} to list");
            }
        }

        public void AddPolicy(PolicyEntity entity)
        {
            var value = JsonSerializer.SerializeToUtf8Bytes(entity, _options);

            try
            {
                _database.Put(entity.Id, value);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to add policy info for {entity.Id}");
            }
        }

        public void RemovePolicy(Guid policyId)
        {
            try
            {
                var policyIds = GetAllPoliciesIds();

                for (int i = 0; i < policyIds.Count; i++)
                    if (new Guid(policyIds[i]) == policyId)
                    {
                        policyIds.RemoveAt(i);
                        break;
                    }

                _database.Put(_policyIdsKey, JsonSerializer.SerializeToUtf8Bytes(policyIds));
                _database.Delete(policyId.ToByteArray());
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to remove Policy by {policyId}");
            }
        }

        public List<byte[]> GetAllPoliciesIds() => GetListOfBytes(_policyIdsKey, "Failed to get all policy ids");

        public PolicyEntity GetPolicy(byte[] policyId)
        {
            try
            {
                return _database.TryRead(policyId, out byte[] value)
                       ? JsonSerializer.Deserialize<PolicyEntity>(value)
                       : null;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to read info for policy {policyId}");
            }

            return null;
        }

        #endregion

        #region User

        public void AddUser(UserEntity user)
        {
            var userKey = PrefixConstants.GetUniqueUserKey(user.UserName);
            var keyBytes = Encoding.UTF8.GetBytes(userKey);

            try
            {
                _database.Put(keyBytes, JsonSerializer.SerializeToUtf8Bytes(user));
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to save user {user.UserName}");
            }
        }

        public List<UserEntity> ReadUsers()
        {
            var key = PrefixConstants.GetUsersReadKey();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            List<UserEntity> users = new List<UserEntity>();
            try
            {
                List<byte[]> values = _database.GetAllStartingWith(keyBytes);
                foreach (var value in values)
                {
                    try
                    {
                        users.Add(JsonSerializer.Deserialize<UserEntity>(Encoding.UTF8.GetString(value)));
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, $"Failed to deserialize {Encoding.UTF8.GetString(value)} to UserEntity");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to read users!");
            }

            return users;
        }

        public void RemoveUser(UserEntity user)
        {
            var userKey = PrefixConstants.GetUniqueUserKey(user.UserName);
            var keyBytes = Encoding.UTF8.GetBytes(userKey);
            try
            {
                _database.Delete(keyBytes);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to delete user '{user.UserName}'");
            }
        }

        public List<UserEntity> ReadUsersPage(int page, int pageSize)
        {
            var key = PrefixConstants.GetUsersReadKey();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            List<UserEntity> users = new List<UserEntity>();
            try
            {
                List<byte[]> values = _database.GetPageStartingWith(keyBytes, page, pageSize);
                foreach (var value in values)
                {
                    try
                    {
                        users.Add(JsonSerializer.Deserialize<UserEntity>(Encoding.UTF8.GetString(value)));
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, $"Failed to deserialize {Encoding.UTF8.GetString(value)} to UserEntity");
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to read users!");
            }

            return users;
        }

        #endregion

        #region Telegram chats

        public List<byte[]> GetTelegramChatsList() => GetListOfBytes(_telegramChatIdsKey, "Failed to get telegram chats ids list");

        public TelegramChatEntity GetTelegramChat(byte[] chatId)
        {
            try
            {
                return _database.TryRead(chatId, out byte[] value)
                    ? JsonSerializer.Deserialize<TelegramChatEntity>(Encoding.UTF8.GetString(value))
                    : null;
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to read info for telegram chat {new Guid(chatId)}");
            }

            return null;
        }

        public void AddTelegramChat(TelegramChatEntity chat)
        {
            try
            {
                _database.Put(chat.Id, JsonSerializer.SerializeToUtf8Bytes(chat));
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to add telegram chat info for {chat.Id}");
            }
        }

        public void RemoveTelegramChat(byte[] chatId)
        {
            try
            {
                _database.Delete(chatId);
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to remove info for telegram chat {new Guid(chatId)}");
            }
        }

        public void AddTelegramChatToList(byte[] chatId)
        {
            try
            {
                var currentList = GetTelegramChatsList();

                if (!currentList.Contains(chatId))
                    currentList.Add(chatId);

                _database.Put(_telegramChatIdsKey, JsonSerializer.SerializeToUtf8Bytes(currentList));
            }
            catch (Exception e)
            {
                _logger.Error(e, "Failed to add telegram chat id to list");
            }
        }

        public void RemoveTelegramChatFromList(byte[] chatId)
        {
            try
            {
                var currentList = GetTelegramChatsList();

                currentList.Remove(chatId);

                _database.Put(_telegramChatIdsKey, JsonSerializer.SerializeToUtf8Bytes(currentList));
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Failed to remove telegram chat id {chatId} from list");
            }
        }

        #endregion

        public void Dispose() => _database.Dispose();

        private List<string> GetListOfKeys(byte[] key, string error)
        {
            try
            {
                return _database.TryRead(key, out byte[] value) ?
                    JsonSerializer.Deserialize<List<string>>(Encoding.UTF8.GetString(value))
                    : new();
            }
            catch (Exception e)
            {
                _logger.Error(e, error);
            }

            return new();
        }

        private List<byte[]> GetListOfBytes(byte[] key, string error)
        {
            try
            {
                return _database.TryRead(key, out byte[] value) ?
                    JsonSerializer.Deserialize<List<byte[]>>(Encoding.UTF8.GetString(value))
                    : new();
            }
            catch (Exception e)
            {
                _logger.Error(e, error);
            }

            return new();
        }
    }
}
