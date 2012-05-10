﻿using FloydPink.Flickr.Downloadr.Extensions;
using FloydPink.Flickr.Downloadr.Model;

namespace FloydPink.Flickr.Downloadr.Repository
{
    public class TokenRepository : RepositoryBase, IRepository<Token>
    {
        internal override string repoFileName
        {
            get { return "token.repo"; }
        }

        public Token Get()
        {
            return base.Read().FromJson<Token>();
        }

        public void Save(Token value)
        {
            base.Write(value.ToJson());
        }
    }
}
