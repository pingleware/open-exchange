-- #win vs #lose
select sum(if(Winner='ZIP', 1, 0)) as ZIPWins, sum(if(Winner='GDX', 1, 0)) as GDXWins
from
(
  select datesig, simid,
  avg(0.5*(gdxbeff+gdxseff)) as GDXEff,
  avg(0.5*(zipbeff+zipseff)) as ZIPEff,
  if(avg(0.5*(gdxbeff+gdxseff))>avg(0.5*(zipbeff+zipseff)), 'GDX', 'ZIP') as Winner
  from
  (
    select datesig, simid, phasestart, phaseend,
    max(if(usertype='GDXB', 1.0, 0.0)*eff) as GDXBeff,
    max(if(usertype='GDXS', 1.0, 0.0)*eff) as GDXSeff,
    max(if(usertype='ZIPB', 1.0, 0.0)*eff) as ZIPBeff,
    max(if(usertype='ZIPS', 1.0, 0.0)*eff) as ZIPSeff
    from
    (
      select simid, datesig, phasestart, phaseend, usertype, avg(efficiencyperphase) as eff
      from
      (
        select simid, datesig, phasestart, phaseend, user, sum(Profit)/maxthsplus as EfficiencyPerPhase,
        if(user like 'GDX_B%', 'GDXB', if(user like 'GDX_S%', 'GDXS', if(user like 'ZIP_B%', 'ZIPB', 'ZIPS'))) as UserType
        from
        (
          select s.simid, s.datesig, s.phasestart, s.phaseend, s.maxthsplus, abs(f.fillprice - f.limitprice) as Profit, f.user, f.counterparty
          from
          Fillview f,
          (
            select s.simid, s.datesig, s.phasestart, s.phaseend, s.phaseid, s.username, s.maxthsplus
            from SimulationSummary s, batch b
            where b.bid=1
            and s.datesig=b.datesig
            and s.simid=b.simid
          ) s
          where f.timesig between s.phasestart and s.phaseend
          and f.datesig = s.datesig
          and s.username = f.user
        ) ff
        group by datesig, simid, phasestart, phaseend, user
      ) x
      group by simid, datesig, phasestart, phaseend, usertype
    ) y
    group by simid, datesig, phasestart, phaseend
  ) z
  group by datesig, simid
) w;


-- total results
select datesig, simid,
avg(0.5*(gdxbeff+gdxseff)) as GDXEff,
avg(0.5*(zipbeff+zipseff)) as ZIPEff,
if(avg(0.5*(gdxbeff+gdxseff))>avg(0.5*(zipbeff+zipseff)), 'GDX', 'ZIP') as Winner
from
(
  select datesig, simid, phasestart, phaseend,
  max(if(usertype='GDXB', 1.0, 0.0)*eff) as GDXBeff,
  max(if(usertype='GDXS', 1.0, 0.0)*eff) as GDXSeff,
  max(if(usertype='ZIPB', 1.0, 0.0)*eff) as ZIPBeff,
  max(if(usertype='ZIPS', 1.0, 0.0)*eff) as ZIPSeff
  from
  (
    select simid, datesig, phasestart, phaseend, usertype, avg(efficiencyperphase) as eff
    from
    (
      select simid, datesig, phasestart, phaseend, user, sum(Profit)/maxthsplus as EfficiencyPerPhase,
      if(user like 'GDX_B%', 'GDXB', if(user like 'GDX_S%', 'GDXS', if(user like 'ZIP_B%', 'ZIPB', 'ZIPS'))) as UserType
      from
      (
        select s.simid, s.datesig, s.phasestart, s.phaseend, s.maxthsplus, abs(f.fillprice - f.limitprice) as Profit, f.user, f.counterparty
        from
        Fillview f,
        (
          select s.simid, s.datesig, s.phasestart, s.phaseend, s.phaseid, s.username, s.maxthsplus
          from SimulationSummary s, batch b
          where b.bid=1
          and s.datesig=b.datesig
          and s.simid=b.simid
        ) s
        where f.timesig between s.phasestart and s.phaseend
        and f.datesig = s.datesig
        and s.username = f.user
      ) ff
      group by datesig, simid, phasestart, phaseend, user
    ) x
    group by simid, datesig, phasestart, phaseend, usertype
  ) y
  group by simid, datesig, phasestart, phaseend
) z
group by datesig, simid;


-- partial results
select datesig, simid, phasestart, phaseend,
max(if(usertype='GDXB', 1.0, 0.0)*eff) as GDXBeff,
max(if(usertype='GDXS', 1.0, 0.0)*eff) as GDXSeff,
max(if(usertype='ZIPB', 1.0, 0.0)*eff) as ZIPBeff,
max(if(usertype='ZIPS', 1.0, 0.0)*eff) as ZIPSeff
from
(
  select simid, datesig, phasestart, phaseend, usertype, avg(efficiencyperphase) as eff
  from
  (
    select simid, datesig, phasestart, phaseend, user, sum(Profit)/maxthsplus as EfficiencyPerPhase,
    if(user like 'GDX_B%', 'GDXB', if(user like 'GDX_S%', 'GDXS', if(user like 'ZIP_B%', 'ZIPB', 'ZIPS'))) as UserType
    from
    (
      select s.simid, s.datesig, s.phasestart, s.phaseend, s.maxthsplus, abs(f.fillprice - f.limitprice) as Profit, f.user, f.counterparty
      from
      Fillview f,
      (
        select s.simid, s.datesig, s.phasestart, s.phaseend, s.phaseid, s.username, s.maxthsplus
        from SimulationSummary s, batch b
        where b.bid=1
        and s.datesig=b.datesig
        and s.simid=b.simid
      ) s
      where f.timesig between s.phasestart and s.phaseend
      and f.datesig = s.datesig
      and s.username = f.user
    ) ff
    group by datesig, simid, phasestart, phaseend, user
  ) x
  group by simid, datesig, phasestart, phaseend, usertype
) y
group by simid, datesig, phasestart, phaseend;