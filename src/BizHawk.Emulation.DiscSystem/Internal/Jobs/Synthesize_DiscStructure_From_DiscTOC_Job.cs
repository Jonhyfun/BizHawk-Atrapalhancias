using System;

namespace BizHawk.Emulation.DiscSystem
{
	internal class Synthesize_DiscStructure_From_DiscTOC_Job
	{
		private readonly Disc IN_Disc;

		private readonly DiscTOC TOCRaw;

		public Synthesize_DiscStructure_From_DiscTOC_Job(Disc disc, DiscTOC tocRaw)
		{
			IN_Disc = disc;
			TOCRaw = tocRaw;
		}

		public DiscStructure Result { get; private set; }

		/// <exception cref="InvalidOperationException">first track of <see cref="TOCRaw"/> is not <c>1</c></exception>
		public void Run()
		{
			var dsr = new DiscSectorReader(IN_Disc) { Policy = { DeterministicClearBuffer = false } };

			Result = new DiscStructure();
			var session = new DiscStructure.Session();
			Result.Sessions.Add(null); //placeholder session for reindexing
			Result.Sessions.Add(session);

			session.Number = 1;

			if (TOCRaw.FirstRecordedTrackNumber != 1)
				throw new InvalidOperationException($"Unsupported: {nameof(TOCRaw.FirstRecordedTrackNumber)} != 1");

			//add a lead-in track
			session.Tracks.Add(new DiscStructure.Track
			{
				Number = 0,
				Control = EControlQ.None, //we'll set this later
				LBA = -new Timestamp(99,99,99).Sector //obvious garbage
			});

			int ntracks = TOCRaw.LastRecordedTrackNumber - TOCRaw.FirstRecordedTrackNumber + 1;
			for (int i = 0; i < ntracks; i++)
			{
				var item = TOCRaw.TOCItems[i + 1];
				var track = new DiscStructure.Track
				{
					Number = i + 1,
					Control = item.Control,
					LBA = item.LBA
				};
				session.Tracks.Add(track);

				if (!item.IsData)
					track.Mode = 0;
				else
				{
					//determine the mode by a hardcoded heuristic: check mode of first sector
					track.Mode = dsr.ReadLBA_Mode(track.LBA);
				}

				//determine track length according to... how? It isn't clear.
				//Let's not do this until it's needed.
				//if (i == ntracks - 1)
				//  track.Length = TOCRaw.LeadoutLBA.Sector - track.LBA;
				//else track.Length = (TOCRaw.TOCItems[i + 2].LBATimestamp.Sector - track.LBA);
			}

			//add lead-out track
			session.Tracks.Add(new DiscStructure.Track
			{
				Number = 0xA0, //right?
				//kind of a guess, but not completely
				Control = session.Tracks[session.Tracks.Count -1 ].Control,
				Mode = session.Tracks[session.Tracks.Count - 1].Mode,
				LBA = TOCRaw.LeadoutLBA
			});

			//link track list
			for (int i = 0; i < session.Tracks.Count - 1; i++)
			{
				session.Tracks[i].NextTrack = session.Tracks[i + 1];
			}

			//fix lead-in track type
			//guesses:
			session.Tracks[0].Control = session.Tracks[1].Control;
			session.Tracks[0].Mode = session.Tracks[1].Mode;
		}
	}
}