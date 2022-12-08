/* Mednafen - Multi-system Emulator
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 */

#include <string.h>
#include <assert.h>
#include <stdio.h>
#include <stdlib.h>
#include <math.h>
#include "octoshock.h"
#include "surface.h"
#include "convert.h"

MDFN_Surface::MDFN_Surface()
{
 memset(&format, 0, sizeof(format));

 pixels = NULL;
 pixels8 = NULL;
 pixels16 = NULL;
 palette = NULL;
 pixels_is_external = false;
 pitchinpix = 0;
 w = 0;
 h = 0;
}

MDFN_Surface::MDFN_Surface(void *const p_pixels, const uint32 p_width, const uint32 p_height, const uint32 p_pitchinpix, const MDFN_PixelFormat &nf, const bool alloc_init_pixels)
{
 Init(p_pixels, p_width, p_height, p_pitchinpix, nf, alloc_init_pixels);
}

#if 0
void MDFN_Surface::Resize(const uint32 p_width, const uint32 p_height, const uint32 p_pitchinpix)
{
 void *ptr = (format.bpp == 16) ? pixels16 : pixels;
 uint64 old_asize = ((uint64)pitchinpix * (format.bpp >> 3)) * h;
 uint64 new_asize = ((uint64)p_pitchinpix * (format.bpp >> 3)) * p_height;

 if(!(ptr = realloc(ptr, new_asize)))
  throw MDFN_Error(ErrnoHolder(errno));

 if(new_asize > old_asize)
  memset((uint8*)ptr + old_asize, 0x00, new_asize - old_asize);

 if(format.bpp == 16)
  pixels16 = (uint16*)ptr;
 else
  pixels = (uint32*)ptr;

 pitchinpix = p_pitchinpix;
 w = p_width;
 h = p_height;
}
#endif

void MDFN_Surface::Init(void *const p_pixels, const uint32 p_width, const uint32 p_height, const uint32 p_pitchinpix, const MDFN_PixelFormat &nf, const bool alloc_init_pixels)
{
 void *rpix = NULL;
 assert(nf.opp == 1 || nf.opp == 2 || nf.opp == 4);

 format = nf;

 if(nf.opp == 1)
 {
  //assert(!nf.Rshift && !nf.Gshift && !nf.Bshift && !nf.Ashift);
  //assert(!nf.Rprec && !nf.Gprec && !nf.Bprec && !nf.Aprec);
 }
 else if(nf.opp == 2)
 {
  assert(nf.Rprec && nf.Gprec && nf.Bprec && nf.Aprec);
 }
 else
 {
  assert((nf.Rshift + nf.Gshift + nf.Bshift + nf.Ashift) == 48);
  assert(!((nf.Rshift | nf.Gshift | nf.Bshift | nf.Ashift) & 0x7));

  format.Rprec = 8;
  format.Gprec = 8;
  format.Bprec = 8;
  format.Aprec = 8;
 }

 pixels16 = NULL;
 pixels8 = NULL;
 pixels = NULL;
 palette = NULL;

 pixels_is_external = false;

 if(p_pixels)
 {
  rpix = p_pixels;
  pixels_is_external = true;
 }
 else
 {
  if(alloc_init_pixels)
   rpix = calloc(1, p_pitchinpix * p_height * nf.opp);
  else
   rpix = malloc(p_pitchinpix * p_height * nf.opp);

  if(!rpix)
  {
   //ErrnoHolder ene(errno);

		throw "OLD ERROR";
   //throw(MDFN_Error(ene.Errno(), "%s", ene.StrError()));
  }
 }

 if(nf.opp == 1)
 {
  if(!(palette = (MDFN_PaletteEntry*) calloc(sizeof(MDFN_PaletteEntry), 256)))
  {
   //ErrnoHolder ene(errno);

   if(!pixels_is_external)
    free(rpix);

	 throw "OLD ERROR";
   //throw(MDFN_Error(ene.Errno(), "%s", ene.StrError()));
  }
 }

 if(nf.opp == 2)
  pixels16 = (uint16 *)rpix;
 else if(nf.opp == 1)
  pixels8 = (uint8 *)rpix;
 else
  pixels = (uint32 *)rpix;

 w = p_width;
 h = p_height;

 pitchinpix = p_pitchinpix;
}

const uint8 MDFN_PixelFormat::LUT5to8[32] =
{
 0x00, 0x08, 0x10, 0x18, 0x20, 0x29, 0x31, 0x39, 0x41, 0x4a, 0x52, 0x5a, 0x62, 0x6a, 0x73, 0x7b,
 0x83, 0x8b, 0x94, 0x9c, 0xa4, 0xac, 0xb4, 0xbd, 0xc5, 0xcd, 0xd5, 0xde, 0xe6, 0xee, 0xf6, 0xff
};

const uint8 MDFN_PixelFormat::LUT6to8[64] =
{
 0x00, 0x04, 0x08, 0x0c, 0x10, 0x14, 0x18, 0x1c, 0x20, 0x24, 0x28, 0x2c, 0x30, 0x34, 0x38, 0x3c, 
 0x40, 0x44, 0x48, 0x4c, 0x50, 0x55, 0x59, 0x5d, 0x61, 0x65, 0x69, 0x6d, 0x71, 0x75, 0x79, 0x7d, 
 0x81, 0x85, 0x89, 0x8d, 0x91, 0x95, 0x99, 0x9d, 0xa1, 0xa5, 0xaa, 0xae, 0xb2, 0xb6, 0xba, 0xbe, 
 0xc2, 0xc6, 0xca, 0xce, 0xd2, 0xd6, 0xda, 0xde, 0xe2, 0xe6, 0xea, 0xee, 0xf2, 0xf6, 0xfa, 0xff
};

const uint8 MDFN_PixelFormat::LUT8to5[256] =
{
 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x02, 0x02, 0x02, 
 0x02, 0x02, 0x02, 0x02, 0x02, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x03, 0x04, 0x04, 0x04, 
 0x04, 0x04, 0x04, 0x04, 0x04, 0x04, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0x06, 0x06, 
 0x06, 0x06, 0x06, 0x06, 0x06, 0x06, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x07, 0x08, 0x08, 
 0x08, 0x08, 0x08, 0x08, 0x08, 0x08, 0x09, 0x09, 0x09, 0x09, 0x09, 0x09, 0x09, 0x09, 0x09, 0x0a, 
 0x0a, 0x0a, 0x0a, 0x0a, 0x0a, 0x0a, 0x0a, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0b, 0x0c, 
 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0c, 0x0d, 0x0d, 0x0d, 0x0d, 0x0d, 0x0d, 0x0d, 0x0d, 0x0d, 
 0x0e, 0x0e, 0x0e, 0x0e, 0x0e, 0x0e, 0x0e, 0x0e, 0x0f, 0x0f, 0x0f, 0x0f, 0x0f, 0x0f, 0x0f, 0x0f, 
 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x10, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 0x11, 
 0x12, 0x12, 0x12, 0x12, 0x12, 0x12, 0x12, 0x12, 0x12, 0x13, 0x13, 0x13, 0x13, 0x13, 0x13, 0x13, 
 0x13, 0x14, 0x14, 0x14, 0x14, 0x14, 0x14, 0x14, 0x14, 0x15, 0x15, 0x15, 0x15, 0x15, 0x15, 0x15, 
 0x15, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x16, 0x17, 0x17, 0x17, 0x17, 0x17, 0x17, 
 0x17, 0x17, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x18, 0x19, 0x19, 0x19, 0x19, 0x19, 0x19, 
 0x19, 0x19, 0x1a, 0x1a, 0x1a, 0x1a, 0x1a, 0x1a, 0x1a, 0x1a, 0x1b, 0x1b, 0x1b, 0x1b, 0x1b, 0x1b, 
 0x1b, 0x1b, 0x1b, 0x1c, 0x1c, 0x1c, 0x1c, 0x1c, 0x1c, 0x1c, 0x1c, 0x1d, 0x1d, 0x1d, 0x1d, 0x1d, 
 0x1d, 0x1d, 0x1d, 0x1e, 0x1e, 0x1e, 0x1e, 0x1e, 0x1e, 0x1e, 0x1e, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f
};

const uint8 MDFN_PixelFormat::LUT8to6[256] =
{
 0x00, 0x00, 0x00, 0x01, 0x01, 0x01, 0x01, 0x02, 0x02, 0x02, 0x02, 0x03, 0x03, 0x03, 0x03, 0x04, 
 0x04, 0x04, 0x04, 0x05, 0x05, 0x05, 0x05, 0x06, 0x06, 0x06, 0x06, 0x07, 0x07, 0x07, 0x07, 0x08, 
 0x08, 0x08, 0x08, 0x09, 0x09, 0x09, 0x09, 0x0a, 0x0a, 0x0a, 0x0a, 0x0b, 0x0b, 0x0b, 0x0b, 0x0c, 
 0x0c, 0x0c, 0x0c, 0x0d, 0x0d, 0x0d, 0x0d, 0x0e, 0x0e, 0x0e, 0x0e, 0x0f, 0x0f, 0x0f, 0x0f, 0x10, 
 0x10, 0x10, 0x10, 0x11, 0x11, 0x11, 0x11, 0x12, 0x12, 0x12, 0x12, 0x13, 0x13, 0x13, 0x13, 0x14, 
 0x14, 0x14, 0x14, 0x15, 0x15, 0x15, 0x15, 0x15, 0x16, 0x16, 0x16, 0x16, 0x17, 0x17, 0x17, 0x17, 
 0x18, 0x18, 0x18, 0x18, 0x19, 0x19, 0x19, 0x19, 0x1a, 0x1a, 0x1a, 0x1a, 0x1b, 0x1b, 0x1b, 0x1b, 
 0x1c, 0x1c, 0x1c, 0x1c, 0x1d, 0x1d, 0x1d, 0x1d, 0x1e, 0x1e, 0x1e, 0x1e, 0x1f, 0x1f, 0x1f, 0x1f, 
 0x20, 0x20, 0x20, 0x20, 0x21, 0x21, 0x21, 0x21, 0x22, 0x22, 0x22, 0x22, 0x23, 0x23, 0x23, 0x23, 
 0x24, 0x24, 0x24, 0x24, 0x25, 0x25, 0x25, 0x25, 0x26, 0x26, 0x26, 0x26, 0x27, 0x27, 0x27, 0x27, 
 0x28, 0x28, 0x28, 0x28, 0x29, 0x29, 0x29, 0x29, 0x2a, 0x2a, 0x2a, 0x2a, 0x2a, 0x2b, 0x2b, 0x2b, 
 0x2b, 0x2c, 0x2c, 0x2c, 0x2c, 0x2d, 0x2d, 0x2d, 0x2d, 0x2e, 0x2e, 0x2e, 0x2e, 0x2f, 0x2f, 0x2f, 
 0x2f, 0x30, 0x30, 0x30, 0x30, 0x31, 0x31, 0x31, 0x31, 0x32, 0x32, 0x32, 0x32, 0x33, 0x33, 0x33, 
 0x33, 0x34, 0x34, 0x34, 0x34, 0x35, 0x35, 0x35, 0x35, 0x36, 0x36, 0x36, 0x36, 0x37, 0x37, 0x37, 
 0x37, 0x38, 0x38, 0x38, 0x38, 0x39, 0x39, 0x39, 0x39, 0x3a, 0x3a, 0x3a, 0x3a, 0x3b, 0x3b, 0x3b, 
 0x3b, 0x3c, 0x3c, 0x3c, 0x3c, 0x3d, 0x3d, 0x3d, 0x3d, 0x3e, 0x3e, 0x3e, 0x3e, 0x3f, 0x3f, 0x3f
};

// When we're converting, only convert the w*h area(AKA leave the last part of the line, pitch32 - w, alone),
// for places where we store auxillary information there(graphics viewer in the debugger), and it'll be faster
// to boot.
void MDFN_Surface::SetFormat(const MDFN_PixelFormat &nf, bool convert)
{
 if(format == nf)
  return;
 //
 void* old_pixels = nullptr;
 void* new_pixels = nullptr;
 void* new_palette = nullptr;

 switch(format.opp)
 {
  case 1: old_pixels = pix<uint8>(); break;
  case 2: old_pixels = pix<uint16>(); break;
  case 4: old_pixels = pix<uint32>(); break;
 }

 if(nf.opp != format.opp)
 {
  new_pixels = calloc(1, pitchinpix * h * nf.opp);

  if(nf.opp == 1)
   new_palette = calloc(sizeof(MDFN_PaletteEntry), 256);
 }
 //
 //
 if(convert)
 {
  MDFN_PixelFormatConverter fconv(format, nf, (MDFN_PaletteEntry*)(palette ? palette : new_palette));
  size_t old_pitchinbytes = pitchinpix * format.opp;
  size_t new_pitchinbytes = pitchinpix * nf.opp;

  if(new_pixels)
  {
   for(int32 y = 0; y < h; y++)
    fconv.Convert((uint8*)old_pixels + y * old_pitchinbytes, (uint8*)new_pixels + y * new_pitchinbytes, w);
  }
  else
  {
   for(int32 y = 0; y < h; y++)
    fconv.Convert((uint8*)old_pixels + y * old_pitchinbytes, w);
  }
 }
 //
 //
 if(nf.opp != format.opp)
 {
  switch(format.opp)
  {
   case 1: pixels8 = nullptr; break;
   case 2: pixels16 = nullptr; break;
   case 4: pixels = nullptr; break;
  }

  if(palette)
  {
   free(palette);
   palette = nullptr;
  }

  if(!pixels_is_external)
   free(old_pixels);

  pixels_is_external = false;

  switch(nf.opp)
  {
   case 1: pixels8 = (uint8*)new_pixels; palette = (MDFN_PaletteEntry*)new_palette; break;
   case 2: pixels16 = (uint16*)new_pixels; break;
   case 4: pixels = (uint32*)new_pixels; break;
  }
 }

 format = nf;
}

void MDFN_Surface::Fill(uint8 r, uint8 g, uint8 b, uint8 a)
{
 uint32 color = MakeColor(r, g, b, a);

 if(format.opp == 1)
 {
  assert(pixels8);

  for(int32 i = 0; i < pitchinpix * h; i++)
   pixels8[i] = color;
 }
 else if(format.opp == 2)
 {
  assert(pixels16);

  for(int32 i = 0; i < pitchinpix * h; i++)
   pixels16[i] = color;
 }
 else
 {
  assert(pixels);

  for(int32 i = 0; i < pitchinpix * h; i++)
   pixels[i] = color;
 }
}

MDFN_Surface::~MDFN_Surface()
{
 if(!pixels_is_external)
 {
  if(pixels)
   free(pixels);
  if(pixels16)
   free(pixels16);
  if(pixels8)
   free(pixels8);
  if(palette)
   free(palette);
 }
}
