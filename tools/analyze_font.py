#!/usr/bin/env python3
# analyze_font.py
# Requires: pip install fonttools

import sys
from fontTools import ttLib
from collections import OrderedDict

def dump_cmap(tt):
	print("=== cmap table ===")
	cmap = tt['cmap']
	for subtable in cmap.tables:
		print(f"platformID={subtable.platformID}, platEncID={subtable.platEncID}, format={subtable.format}, language={getattr(subtable,'language',None)}")
	unified = {}
	for subtable in cmap.tables:
		if hasattr(subtable, 'cmap'):
			unified.update(subtable.cmap)
	return unified

def dump_ot_features(tt):
	print("\n=== GSUB features/Lookups summary ===")
	if 'GSUB' in tt:
		gsub = tt['GSUB'].table
		if hasattr(gsub, 'FeatureList') and gsub.FeatureList:
			for featRecord in gsub.FeatureList.FeatureRecord:
				print("Feature:", featRecord.FeatureTag)
		else:
			print("No FeatureList in GSUB")
		if hasattr(gsub,'LookupList') and gsub.LookupList:
			for i, lookup in enumerate(gsub.LookupList.Lookup):
				print(f"Lookup {i}: LookupType={lookup.LookupType}, LookupFlag={lookup.LookupFlag}, SubTable count={len(lookup.SubTable)}")
				for sti, st in enumerate(lookup.SubTable):
					print("  SubTable", sti, type(st).__name__)
	else:
		print("No GSUB table")

	print("\n=== GPOS features/Lookups summary ===")
	if 'GPOS' in tt:
		gpos = tt['GPOS'].table
		if hasattr(gpos, 'FeatureList') and gpos.FeatureList:
			for featRecord in gpos.FeatureList.FeatureRecord:
				print("Feature:", featRecord.FeatureTag)
		else:
			print("No FeatureList in GPOS")
		if hasattr(gpos,'LookupList') and gpos.LookupList:
			for i, lookup in enumerate(gpos.LookupList.Lookup):
				print(f"GPOS Lookup {i}: LookupType={lookup.LookupType}, SubTable count={len(lookup.SubTable)}")
	else:
		print("No GPOS table")

def dump_post(tt):
	print("\n=== post table ===")
	post = tt['post']
	print("post format", post.formatType)

def dump_glyf_info(tt):
	print("\n=== glyf table (glyph count) ===")
	if 'glyf' in tt:
		print("Number of glyphs:", len(tt.getGlyphOrder()))
	else:
		print("No glyf table (maybe CFF)")

def list_pua_mappings(cmap, start=0xF100, end=0xF500):
	print(f"\n=== Mapping U+{start:04X}..U+{end:04X} ===")
	found = OrderedDict()
	for cp in range(start, end+1):
		if cp in cmap:
			found[cp] = cmap[cp]
	if not found:
		print("No mappings in this range.")
	else:
		for cp,gname in found.items():
			print(f"U+{cp:04X}\t->\t{gname}")
	return found

def inspect_gsub_for_reordering(tt):
	print("\n=== Inspect GSUB lookups for potential reordering ===")
	if 'GSUB' not in tt:
		print("No GSUB")
		return
	gsub = tt['GSUB'].table
	lookups = getattr(gsub, 'LookupList', None)
	if not lookups:
		print("No lookups")
		return
	for i, lookup in enumerate(lookups.Lookup):
		lt = lookup.LookupType
		if lt == 8:
			print(f"Lookup {i} is ReverseChainSingleSubst (type 8) -> can reorder glyphs")
		elif lt in (5,6):
			print(f"Lookup {i} is Contextual/Chaining (type {lt}) -> may perform sequence substitutions")
		elif lt == 4:
			print(f"Lookup {i} is LigatureSubst (type 4) -> ligatures may collapse sequences")
	print("To fully map features->lookups inspect FeatureList/LookupList in ttx output.")

def list_features_present(tt):
	feats = set()
	for tableTag in ('GSUB','GPOS'):
		if tableTag in tt:
			table = tt[tableTag].table
			fl = getattr(table, 'FeatureList', None)
			if fl:
				for fr in fl.FeatureRecord:
					feats.add(fr.FeatureTag)
	print("\n=== Features present (GSUB/GPOS) ===")
	for f in sorted(feats):
		print(f)
	return feats

def main(path):
	tt = ttLib.TTFont(path)
	cmap = dump_cmap(tt)
	dump_ot_features(tt)
	dump_post(tt)
	dump_glyf_info(tt)
	mapped = list_pua_mappings(cmap)
	feats = list_features_present(tt)
	inspect_gsub_for_reordering(tt)

	print("\n=== Detailed mapping for found PUA entries ===")
	for cp,gname in mapped.items():
		try:
			glyphID = tt.getGlyphID(gname)
		except Exception:
			glyphID = None
		print(f"U+{cp:04X}\tGlyphName={gname}\tGlyphID={glyphID}")

if __name__ == '__main__':
	if len(sys.argv) < 2:
		print("Usage: analyze_font.py path/to/font.ttf")
		sys.exit(1)
	main(sys.argv[1])
