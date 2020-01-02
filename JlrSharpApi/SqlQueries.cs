using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using GreyMan.JlrSharp;

namespace JlrSharpApi
{
    public static class SqlQueries
    {
        // If the app ever got scaled, this might break
        public static SqlConnection Sql = new SqlConnection(Environment.GetEnvironmentVariable("JlrSharpDbConnection"));

        static SqlQueries()
        {
            Sql.Open();
        }

        /// <summary>
        /// Retrieves an authorised user by email address
        /// </summary>
        public static AuthorisedUser GetAuthorisedUserByEmail(string email)
        {
            // Set up T-SQL query
            string findUserQuery = $"SELECT * FROM JlrUsers WHERE EmailAddress=\'{email}\'";
            return GetAuthorisedUserByQuery(findUserQuery);
        }

        /// <summary>
        /// Retrieves an authorised user by authentication_token
        /// </summary>
        public static AuthorisedUser GetAuthorisedUserByAuthorisationToken(string authorization_token)
        {
            // Set up T-SQL query
            string findUserQuery = $"SELECT * FROM JlrUsers WHERE authorization_token=\'{authorization_token}\'";
            return GetAuthorisedUserByQuery(findUserQuery);
        }

        /// <summary>
        /// Retrieves an authorised user by access_token
        /// </summary>
        public static AuthorisedUser GetAuthorisedUserByAccessToken(string access_token)
        {
            // Set up T-SQL query
            string findUserQuery = $"SELECT * FROM JlrUsers WHERE access_token=\'{access_token}\'";
            return GetAuthorisedUserByQuery(findUserQuery);
        }

        /// <summary>
        /// Retrieves an authorised user by refresh_token
        /// </summary>
        public static AuthorisedUser GetAuthorisedUserByRefreshToken(string refresh_token)
        {
            // Set up T-SQL query
            string findUserQuery = $"SELECT * FROM JlrUsers WHERE refresh_token=\'{refresh_token}\'";
            return GetAuthorisedUserByQuery(findUserQuery);
        }

        /// <summary>
        /// Generic method for returning a user
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private static AuthorisedUser GetAuthorisedUserByQuery(string query)
        {
            using (SqlCommand findUserCmd = new SqlCommand(query, Sql))
            {
                SqlDataReader reader = findUserCmd.ExecuteReader();
                
                // The query returned no rows
                if (!reader.HasRows)
                {
                    return null;
                }

                reader.Read(); // We only expect a single result

                // Create bespoke Alexa response (not sure if we could just return everything)
                return new AuthorisedUser
                {
                    UserInfo = new UserDetails
                    {
                        Email = reader["EmailAddress"].ToString(),
                        Pin = reader["PinCode"].ToString(),
                        UserId = reader["UserId"].ToString(),
                        DeviceId = Guid.Parse(reader["DeviceId"].ToString()),
                        DeviceIdExpiry = DateTime.Parse(reader["DeviceIdExpiry"].ToString())
                    },
                    TokenData = new TokenStore
                    {
                        access_token = reader["access_token"].ToString(),
                        token_type = reader["token_type"].ToString(),
                        expires_in = reader["expires_in"].ToString(),
                        refresh_token = reader["refresh_token"].ToString(),
                        CreatedDate = DateTime.Parse(reader["CreatedDate"].ToString())
                    }
                };
            }
        }

        /// <summary>
        /// Inserts a new user into the database
        /// </summary>
        /// <param name="userDetails"></param>
        /// <param name="tokenStore"></param>
        /// <returns></returns>
        public static bool InsertNewUser(UserDetails userDetails, TokenStore tokenStore)
        {
            // Insert the user into our database
            string addUser = @"INSERT INTO dbo.JlrUsers 
                                    (EmailAddress,PinCode,UserId,DeviceId,DeviceIdExpiry,access_token,authorization_token,expires_in,refresh_token,token_type,CreatedDate) 
                                    VALUES (@EmailAddress,@PinCode,@UserId,@DeviceId,@DeviceIdExpiry,@access_token,@authorization_token,@expires_in,@refresh_token,@token_type,@CreatedDate)";

            using (SqlCommand insertUser = new SqlCommand(addUser, Sql))
            {
                insertUser.Parameters.AddWithValue("@EmailAddress", userDetails.Email);
                insertUser.Parameters.AddWithValue("@PinCode", userDetails.Pin);
                insertUser.Parameters.AddWithValue("@UserId", userDetails.UserId);
                insertUser.Parameters.AddWithValue("@DeviceId", userDetails.DeviceId);
                insertUser.Parameters.AddWithValue("@DeviceIdExpiry", userDetails.DeviceIdExpiry);
                insertUser.Parameters.AddWithValue("@access_token", tokenStore.access_token);
                insertUser.Parameters.AddWithValue("@authorization_token", tokenStore.authorization_token);
                insertUser.Parameters.AddWithValue("@expires_in", tokenStore.expires_in);
                insertUser.Parameters.AddWithValue("@refresh_token", tokenStore.refresh_token);
                insertUser.Parameters.AddWithValue("@token_type", tokenStore.token_type);
                insertUser.Parameters.AddWithValue("@CreatedDate", tokenStore.CreatedDate);

                return insertUser.ExecuteNonQuery() > 0;
            }
        }

        /// <summary>
        /// Deletes a user from the Database
        /// </summary>
        /// <returns></returns>
        public static bool DeleteUserByEmail(string emailAddress)
        {
            string deleteUserQuery = $"DELETE FROM JlrUsers WHERE EmailAddress=\"{emailAddress}\"";
            using (SqlCommand deleteUserCmd = new SqlCommand(deleteUserQuery, Sql))
            {
                return deleteUserCmd.ExecuteNonQuery() > 0;
            }
        }
    }
}
