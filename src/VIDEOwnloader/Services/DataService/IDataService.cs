#region License

// VIDEOwnloader
// Copyright (C) 2016 Adam Rutkowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

#endregion

using System;
using System.Threading.Tasks;
using VIDEOwnloader.Base.Validation;
using VIDEOwnloader.Base.Video;

namespace VIDEOwnloader.Services.DataService
{
    public interface IDataService
    {
        void GetValid(string url, Action<UrlValidationResponse, Exception> callback);
        Task GetValidAsync(string url, Action<UrlValidationResponse, Exception> callback);
        void GetVideo(string url, Action<VideoInfoResponse, Exception> callback);
        Task GetVideoAsync(string url, Action<VideoInfoResponse, Exception> callback);
    }
}