﻿using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System;
using AtlusScriptLib.FlowScriptLanguage.BinaryModel.IO;
using AtlusScriptLib.MessageScriptLanguage.BinaryModel;

namespace AtlusScriptLib.FlowScriptLanguage.BinaryModel
{
    // Todo: ensure immutability
    public sealed class FlowScriptBinary
    {
        public static FlowScriptBinary FromFile( string path )
        {
            return FromFile( path, FlowScriptBinaryFormatVersion.Unknown );
        }

        public static FlowScriptBinary FromFile( string path, FlowScriptBinaryFormatVersion version )
        {
            using ( var fileStream = File.OpenRead( path ) )
                return FromStream( fileStream, version );
        }

        public static FlowScriptBinary FromStream( Stream stream, bool leaveOpen = false )
        {
            return FromStream( stream, FlowScriptBinaryFormatVersion.Unknown, leaveOpen );
        }

        public static FlowScriptBinary FromStream( Stream stream, FlowScriptBinaryFormatVersion version, bool leaveOpen = false )
        {
            using ( var reader = new FlowScriptBinaryReader( stream, version, leaveOpen ) )
            {
                return reader.ReadBinary();
            }
        }

        // these fields are internal because they are used by the builder
        internal FlowScriptBinaryHeader mHeader;
        internal FlowScriptBinarySectionHeader[] mSectionHeaders;
        internal FlowScriptBinaryLabel[] mProcedureLabelSection;
        internal FlowScriptBinaryLabel[] mJumpLabelSection;
        internal FlowScriptBinaryInstruction[] mTextSection;
        internal MessageScriptBinary mMessageScriptSection;
        internal byte[] mStringSection;
        internal FlowScriptBinaryFormatVersion mFormatVersion;

        public FlowScriptBinaryHeader Header
        {
            get { return mHeader; }
        }

        public ReadOnlyCollection<FlowScriptBinarySectionHeader> SectionHeaders
        {
            get
            {
                if ( mSectionHeaders == null )
                    return null;
                else
                    return new ReadOnlyCollection<FlowScriptBinarySectionHeader>( mSectionHeaders );
            }
        }

        public ReadOnlyCollection<FlowScriptBinaryLabel> ProcedureLabelSection
        {
            get
            {
                if ( mProcedureLabelSection == null )
                    return null;
                else
                    return new ReadOnlyCollection<FlowScriptBinaryLabel>( mProcedureLabelSection );
            }
        }

        public ReadOnlyCollection<FlowScriptBinaryLabel> JumpLabelSection
        {
            get
            {
                if ( mJumpLabelSection == null )
                    return null;
                else
                    return new ReadOnlyCollection<FlowScriptBinaryLabel>( mJumpLabelSection );
            }
        }

        public ReadOnlyCollection<FlowScriptBinaryInstruction> TextSection
        {
            get
            {
                if ( mTextSection == null )
                    return null;
                else
                    return new ReadOnlyCollection<FlowScriptBinaryInstruction>( mTextSection );
            }
        }

        public MessageScriptBinary MessageScriptSection
        {
            get { return mMessageScriptSection; }
            set
            {
                // Fixup size
                int sizeDifference = value.Header.FileSize - mMessageScriptSection.Header.FileSize;

                if ( sizeDifference != 0 )
                {
                    var sectionHeaderIndex = Array.FindIndex( mSectionHeaders, x => x.SectionType == FlowScriptBinarySectionType.MessageScriptSection );

                    if ( sectionHeaderIndex != -1 )
                    {
                        mSectionHeaders[sectionHeaderIndex].ElementCount = mMessageScriptSection.Header.FileSize;

                        int lastHeaderIndex = mSectionHeaders.Length - 1;
                        if ( sectionHeaderIndex != lastHeaderIndex )
                        {
                            int numHeadersToFixUp = sectionHeaderIndex - lastHeaderIndex;
                            for ( int i = sectionHeaderIndex + 1; i < numHeadersToFixUp; i++ )
                            {
                                mSectionHeaders[i].FirstElementAddress += sizeDifference;
                            }
                        }
                    }
                    else
                    {
                        throw new NotImplementedException( "Adding a MessageScript section where it does not exist yet is not implemented" );
                    }
                }

                mMessageScriptSection = value;
            }
        }

        public ReadOnlyCollection<byte> StringSection
        {
            get
            {
                if ( mStringSection == null )
                    return null;
                else
                    return new ReadOnlyCollection<byte>( mStringSection );
            }
        }

        public FlowScriptBinaryFormatVersion FormatVersion
        {
            get { return mFormatVersion; }
        }

        // this constructor is internal because it is used by the builder
        internal FlowScriptBinary()
        {
        }

        public void ToFile( string path )
        {
            ToStream( File.Create( path ) );
        }

        public Stream ToStream()
        {
            var stream = new MemoryStream();
            ToStream( stream );
            return stream;
        }

        public void ToStream( Stream stream, bool leaveOpen = false )
        {
            using ( var writer = new FlowScriptBinaryWriter( stream, mFormatVersion ) )
            {
                writer.WriteBinary( this );
            }
        }
    }
}
