﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JMMServer.Entities;
using NHibernate.Criterion;

namespace JMMServer.Repositories
{
	public class AniDB_TagRepository
	{
		public void Save(AniDB_Tag obj)
		{
			using (var session = JMMService.SessionFactory.OpenSession())
			{
				// populate the database
				using (var transaction = session.BeginTransaction())
				{
					session.SaveOrUpdate(obj);
					transaction.Commit();
				}
			}
		}

		public AniDB_Tag GetByID(int id)
		{
			using (var session = JMMService.SessionFactory.OpenSession())
			{
				return session.Get<AniDB_Tag>(id);
			}
		}

		public List<AniDB_Tag> GetAll()
		{
			using (var session = JMMService.SessionFactory.OpenSession())
			{
				var objs = session
					.CreateCriteria(typeof(AniDB_Tag))
					.List<AniDB_Tag>();

				return new List<AniDB_Tag>(objs); ;
			}
		}

		public AniDB_Tag GetByTagID(int id)
		{
			using (var session = JMMService.SessionFactory.OpenSession())
			{
				AniDB_Tag cr = session
					.CreateCriteria(typeof(AniDB_Tag))
					.Add(Restrictions.Eq("TagID", id))
					.UniqueResult<AniDB_Tag>();

				return cr;
			}
		}

		/// <summary>
		/// Gets all the tags, but only if we have the anime locally
		/// </summary>
		/// <returns></returns>
		public List<AniDB_Tag> GetAllForLocalSeries()
		{
			using (var session = JMMService.SessionFactory.OpenSession())
			{
				var tags = session.CreateQuery("FROM AniDB_Tag tag WHERE tag.TagID in (SELECT aat.TagID FROM AniDB_Anime_Tag aat, AnimeSeries aser WHERE aat.AnimeID = aser.AniDB_ID)")
					.List<AniDB_Tag>();

				return new List<AniDB_Tag>(tags);
			}
		}

		public void Delete(int id)
		{
			using (var session = JMMService.SessionFactory.OpenSession())
			{
				// populate the database
				using (var transaction = session.BeginTransaction())
				{
					AniDB_Tag cr = GetByID(id);
					if (cr != null)
					{
						session.Delete(cr);
						transaction.Commit();
					}
				}
			}
		}
	}
}