using BASE.Models;
using BASE.Models.DB;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NPOI.POIFS.FileSystem;
using System.Linq.Expressions;


namespace BASE.Service.Base
{
    public class ServiceBase
    {
        /// <summary> 
        /// Context 連線資訊
        /// </summary>
        public DBContext _context { get; set; } = null!;


        /// <summary>
        /// 建構式
        /// </summary>
        /// <param name="context">Entity Framework Core DbContext物件</param>
        public ServiceBase(DBContext context)
        {
            try
            {
                _context = context;
            }
            catch (Exception ex)
            {

            }
        }

        #region 查詢 Query

        /// <summary> 
        /// 取得資料
        /// </summary>
        /// <typeparam name="T">方法型別</typeparam>
        /// <param name="ErrMsg">錯誤訊息</param>
        /// <param name="filter">LINQ 表達式</param>
        /// <returns></returns>
        public IQueryable<T>? Lookup<T>(ref String ErrMsg, Expression<Func<T, bool>>? filter = null) where T : class
        {
            try
            {
                /* 標記為 NoTracking，防止對取出 Items 做變更後接續 SaveChanges() 動作一併儲存 */
                var dataList = _context.Set<T>().AsNoTracking();

                if (filter != null)
                {
                    dataList = dataList.Where(filter);
                }

                return dataList;
            }
            catch (Exception ex)
            {
                ErrMsg = ErrMsg + "|" + ex.Message;

                return null;
            }
        }

        /// <summary>
        /// 以 Primary Key 取得資料
        /// </summary>
        /// <typeparam name="T">方法型別</typeparam>
        /// <param name="key">型態須正確(不會自動轉型)</param>
        /// <returns></returns>
        public async Task<T?> Find<T>(params object[] key) where T : class
        {
            try
            {
                var data = await _context.Set<T>().FindAsync(key);

                return data;
            }
            catch (Exception ex)
            {

                return null;
            }
        }

        #endregion

        /// <summary>
        /// 讀取對一的相依關聯 (有FK的外部關聯) 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="entity"></param>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        public async Task<ActionResultModel<bool>> LoadReference<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty?>> propertyExpression) where TEntity : class
                                                                                                                                                                where TProperty : class
        {
            ActionResultModel<bool> result = new ActionResultModel<bool>();
            try
            {


                if (entity != null)
                {
                    await _context.Entry(entity).Reference(propertyExpression).LoadAsync();
                }

            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Description = "讀取相依關聯失敗(對一)";
                result.Message = ex.ToString();
            }

            return result;
        }

        /// <summary>
        /// 讀取對多的相依關聯 (有FK的外部關聯) 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="entity"></param>
        /// <param name="propertyExpression"></param>
        /// <returns></returns>
        public async Task<ActionResultModel<bool>> LoadCollection<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, IEnumerable<TProperty>>> propertyExpression) where TEntity : class
                                                                                                                                                                            where TProperty: class
        {
            ActionResultModel<bool> result = new ActionResultModel<bool>();
            try
            {
                if (entity != null)
                {
                    await _context.Entry(entity).Collection(propertyExpression).LoadAsync();
                }

            }
            catch (Exception ex)
            {
                result.IsSuccess = false;
                result.Description = "讀取相依關聯失敗(對多)";
                result.Message = ex.ToString();
            }

            return result;
        }

        #region 新增Insert

        /// <summary>
        /// 新增資料
        /// </summary>
        /// <typeparam name="T">方法型別</typeparam>
        /// <param name="entity">類別</param>
        /// <returns></returns>
        public async Task<ActionResultModel<T>> Insert<T>(T entity, IDbContextTransaction? transaction = null) where T : class
        {
            ActionResultModel<T> result = new ActionResultModel<T>();
            try
            {


                if (entity == null)
                {
                    result.IsSuccess = false;
                    result.Description = "新增失敗";
                    result.Message = "傳入的entity是null";
                }
                else
                {
                    if (transaction != null)
                    {
                        _context.Database.UseTransaction(transaction.GetDbTransaction());
                    }

                    _context.Set<T>().Add(entity);
                    await _context.SaveChangesAsync();

                    result.IsSuccess = true;
                    result.Description = "新增成功";
                    result.Data = entity;
                }
            }
            catch (Exception ex)
            {
                _context.Entry<T>(entity).State = EntityState.Detached;

                if (transaction != null)
                {
                    throw;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Description = "新增失敗";
                    result.Message = ex.ToString();
                }

            }

            return result;
        }

        /// <summary>
        /// 新增資料(多筆)，傳入空陣列 return true、傳入 null return false
        /// </summary>
        /// <typeparam name="T">方法型別</typeparam>
        /// <param name="entities">類別</param>
        /// <returns></returns>
        public async Task<ActionResultModel<IEnumerable<T>>> InsertRange<T>(IEnumerable<T> entities, IDbContextTransaction? transaction = null) where T : class
        {
            ActionResultModel<IEnumerable<T>> result = new ActionResultModel<IEnumerable<T>>();
            try
            {
                if (entities == null)
                {
                    result.IsSuccess = false;
                    result.Description = "新增失敗";
                    result.Message = "傳入的entity是null";
                }
                else if (entities.Count() == 0)
                {
                    result.IsSuccess = true;
                }
                else
                {
                    if (transaction != null)
                    {
                        _context.Database.UseTransaction(transaction.GetDbTransaction());
                    }

                    _context.Set<T>().AddRange(entities);
                    await _context.SaveChangesAsync();

                    result.IsSuccess = true;
                    result.Description = "新增成功";
                    result.Data = entities;
                }
            }
            catch (Exception ex)
            {
                foreach(var entity in entities)
                    _context.Entry<T>(entity).State = EntityState.Detached;

                if (transaction != null)
                {
                    throw;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Description = "新增失敗";
                    result.Message = ex.ToString();
                }
            }
            return result;
        }

        #endregion

        #region 修改 Update

        /// <summary>
        /// 修改資料
        /// </summary>
        /// <typeparam name="T">方法型別</typeparam>
        /// <param name="entity">類別</param>
        /// <returns></returns>
        public async Task<ActionResultModel<T>> Update<T>(T entity, IDbContextTransaction? transaction = null) where T : class
        {
            ActionResultModel<T> result = new ActionResultModel<T>();
            try
            {
                if (entity == null)
                {
                    result.IsSuccess = false;
                    result.Description = "更新失敗";
                    result.Message = "傳入的entity是null";
                }
                else
                {
                    if (transaction != null)
                    {
                        _context.Database.UseTransaction(transaction.GetDbTransaction());
                    }

                    _context.Update(entity);
                    await _context.SaveChangesAsync();

                    result.IsSuccess = true;
                    result.Description = "更新成功";
                    result.Data = entity;
                }
            }
            catch (Exception ex)
            {
                _context.Entry<T>(entity).State = EntityState.Detached;

                if (transaction != null)
                {
                    throw;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Description = "更新失敗";
                    result.Message = ex.ToString();
                }
            }


            return result;
        }

        /// <summary>
        /// 修改資料(多筆)，傳入空陣列 return true、傳入 null return false
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entities"></param>
        /// <param name="ErrMsg"></param>
        /// <returns></returns>
        public async Task<ActionResultModel<IEnumerable<T>>> UpdateRange<T>
            (IEnumerable<T> entities, IDbContextTransaction? transaction = null) where T : class
        {
            ActionResultModel<IEnumerable<T>> result = new ActionResultModel<IEnumerable<T>>();
            try
            {
                if (entities == null)
                {
                    result.IsSuccess = false;
                    result.Description = "更新失敗";
                    result.Message = "傳入的entity是null";
                }
                else if (entities.Count() == 0)
                {
                    result.IsSuccess = true;
                }
                else
                {
                    if (transaction != null)
                    {
                        _context.Database.UseTransaction(transaction.GetDbTransaction());
                    }

                    _context.UpdateRange(entities);
                    await _context.SaveChangesAsync();

                    result.IsSuccess = true;
                    result.Description = "更新成功";
                    result.Data = entities;
                }
            }
            catch (Exception ex)
            {
                foreach(var entity in entities)
                    _context.Entry<T>(entity).State = EntityState.Detached;

                if (transaction != null)
                {
                    throw;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Description = "更新失敗";
                    result.Message = ex.ToString();
                }

            }

            return result;
        }

        #endregion

        #region 刪除

        /// <summary>
        /// 刪除資料
        /// </summary>
        /// <typeparam name="T">方法型別</typeparam>
        /// <param name="entity">類別</param>
        /// <param name="ErrMsg">錯誤訊息</param>
        /// <returns></returns>
        public async Task<ActionResultModel<T>> Delete<T>(T entity, IDbContextTransaction? transaction = null) where T : class
        {
            ActionResultModel<T> result = new ActionResultModel<T>();
            try
            {
                if (entity == null)
                {
                    result.IsSuccess = false;
                    result.Description = "刪除失敗";
                    result.Message = "傳入的entity是null";
                }
                else
                {
                    if (transaction != null)
                    {
                        _context.Database.UseTransaction(transaction.GetDbTransaction());
                    }

                    _context.Remove(entity);
                    await _context.SaveChangesAsync();

                    result.IsSuccess = true;
                    result.Description = "刪除成功";
                    result.Data = entity;
                }
            }
            catch (Exception ex)
            {
                _context.Entry<T>(entity).State = EntityState.Detached;

                if (transaction != null)
                {
                    throw;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Description = "刪除失敗";
                    result.Message = ex.ToString();
                }

            }

            return result;
        }

        /// <summary>
        /// 刪除資料(多筆)，傳入空陣列 return true、傳入 null return false
        /// </summary>
        /// <typeparam name="T">方法型別</typeparam>
        /// <param name="entities">類別</param>
        /// <param name="ErrMsg">錯誤訊息</param>
        /// <returns></returns>
        public async Task<ActionResultModel<IEnumerable<T>>> DeleteRange<T>
            (IEnumerable<T> entities, IDbContextTransaction? transaction = null) where T : class
        {
            ActionResultModel<IEnumerable<T>> result = new ActionResultModel<IEnumerable<T>>();
            try
            {
                if (entities == null)
                {
                    result.IsSuccess = false;
                    result.Description = "刪除失敗";
                    result.Message = "傳入的entity是null";
                }
                else if (entities.Count() == 0)
                {
                    result.IsSuccess = true;
                }
                else
                {
                    if (transaction != null)
                    {
                        _context.Database.UseTransaction(transaction.GetDbTransaction());
                    }

                    _context.RemoveRange(entities);
                    await _context.SaveChangesAsync();

                    result.IsSuccess = true;
                    result.Description = "刪除成功";
                    result.Data = entities;
                }
            }
            catch (Exception ex)
            {
                foreach(var entity in entities)
                    _context.Entry<T>(entity).State = EntityState.Detached;

                if (transaction != null)
                {
                    throw;
                }
                else
                {
                    result.IsSuccess = false;
                    result.Description = "刪除失敗";
                    result.Message = ex.ToString();
                }

            }

            return result;
        }

        #endregion

        #region 改變物件 entity state

        public void SetEntityState<T>(T entity, EntityState state) where T : class
        {
            _context.Entry(entity).State = state;
        }

        public void SetEntitiesState<T>(IEnumerable<T> entities, EntityState state) where T : class
        {
            if (entities != null)
            {
                foreach (T entity in entities)
                {
                    SetEntityState(entity, state);
                }
            }
        }

        #endregion

        public IDbContextTransaction GetTransaction()
        {
            return _context.Database.BeginTransaction();
        }

        /// <summary> 
        ///  純 SQL 執行
        /// </summary>
        /// <param name="query">sql 語法</param>
        /// <returns></returns>
        public async Task<bool> ExecuteSqlCommand(string query)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync(query);

                return true;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        /// <summary> 
        ///  在 Transaction 中鎖定 Table，不可查詢/修改
        /// </summary>
        /// <typeparam name="T">資料庫 Table 型別</typeparam>
        /// <param name="transaction"></param>
        /// <returns></returns>
        public async Task<bool> TableLock<T>(IDbContextTransaction transaction) where T: class
        {
            try
            {
                // 傳入型別名稱
                string table_name = typeof(T).Name;

                _context.Database.UseTransaction(transaction.GetDbTransaction());

                await ExecuteSqlCommand(String.Format("SELECT * FROM {0} WITH (TABLOCKX)", table_name));

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary> 
        ///  在 Transaction 中鎖定 Row(s)，不可查詢/修改
        /// </summary>
        /// <typeparam name="T">資料庫 Table 型別</typeparam>
        /// <param name="transaction"></param>
        /// <param name="condition">where 條件，ex: where UserID = 'U000000001' </param>
        /// <returns></returns>
        public async Task<bool> RowLock<T>(IDbContextTransaction transaction, string condition) where T : class
        {
            try
            {
                // 傳入型別名稱
                string table_name = typeof(T).Name;

                _context.Database.UseTransaction(transaction.GetDbTransaction());

                await ExecuteSqlCommand(String.Format("SELECT * FROM {0} WITH(updlock, rowlock, holdlock) {1}", table_name, condition));

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// 清除_context暫存
        /// insert第一筆_context.SaveChanges()失敗後，原先的_context.Add(entity)並不會被清除，造成後續insert皆會失敗
        /// </summary>
        //public void DetachAllEntities()
        //{
        //    var changedEntriesCopy = _context.ChangeTracker.Entries()
        //        .Where(e => e.State == EntityState.Added ||
        //                    e.State == EntityState.Modified ||
        //                    e.State == EntityState.Deleted)
        //        .ToList();

        //    foreach (var entry in changedEntriesCopy)
        //        entry.State = EntityState.Detached;
        //}

    }
}
