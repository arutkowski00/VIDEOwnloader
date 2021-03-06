﻿#region License

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

namespace VIDEOwnloader.Base.Video
{
    public interface IVideoInfoResultItem
    {
        string Extractor { get; set; }
        string ExtractorKey { get; set; }
        string Id { get; set; }
        string Title { get; set; }
        string WebpageUrl { get; set; }
        string WebpageUrlBasename { get; set; }
    }
}