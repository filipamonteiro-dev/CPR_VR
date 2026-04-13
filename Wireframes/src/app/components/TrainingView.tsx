import { useState, useEffect, useRef } from "react";
import type { ReactNode } from "react";
import { useLocation } from "react-router";
import { motion, AnimatePresence } from "motion/react";
import { WFBackground } from "./WFBackground";
import { ScreenNav } from "./ScreenNav";
import { PauseMenu } from "./PauseMenu";
import { PatientSilhouette } from "./PatientSilhouette";

// ECG waveform generator
function generateECGSegment(offset: number): number {
  const p = ((offset % 100) + 100) % 100;
  if (p < 15) return 0;
  if (p < 20) return Math.sin(((p - 15) / 5) * Math.PI) * 6;
  if (p < 30) return 0;
  if (p < 33) return -4;
  if (p < 37) return 28;
  if (p < 40) return -6;
  if (p < 50) return Math.sin(((p - 40) / 10) * Math.PI) * 9;
  return 0;
}

function ECGDisplay({ bpm }: { bpm: number }) {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const offsetRef = useRef(0);
  const dataRef = useRef<number[]>(Array(200).fill(0));

  useEffect(() => {
    const speed = (bpm / 60) * 1.2;
    let animId: number;

    const draw = () => {
      const canvas = canvasRef.current;
      if (!canvas) return;
      const ctx = canvas.getContext("2d");
      if (!ctx) return;

      offsetRef.current += speed * 0.4;
      const newVal = generateECGSegment(offsetRef.current);
      dataRef.current.push(newVal);
      dataRef.current.shift();

      ctx.clearRect(0, 0, canvas.width, canvas.height);

      // Grid
      ctx.strokeStyle = "rgba(255,255,255,0.05)";
      ctx.lineWidth = 0.5;
      for (let x = 0; x < canvas.width; x += 20) {
        ctx.beginPath(); ctx.moveTo(x, 0); ctx.lineTo(x, canvas.height); ctx.stroke();
      }
      for (let y = 0; y < canvas.height; y += 12) {
        ctx.beginPath(); ctx.moveTo(0, y); ctx.lineTo(canvas.width, y); ctx.stroke();
      }

      // Baseline
      ctx.strokeStyle = "rgba(255,255,255,0.08)";
      ctx.setLineDash([4, 4]);
      ctx.lineWidth = 0.8;
      ctx.beginPath();
      ctx.moveTo(0, canvas.height / 2);
      ctx.lineTo(canvas.width, canvas.height / 2);
      ctx.stroke();
      ctx.setLineDash([]);

      // ECG line
      const gradient = ctx.createLinearGradient(0, 0, canvas.width, 0);
      gradient.addColorStop(0, "rgba(255,255,255,0)");
      gradient.addColorStop(0.4, "rgba(255,255,255,0.5)");
      gradient.addColorStop(1, "rgba(255,255,255,0.92)");
      ctx.strokeStyle = gradient;
      ctx.lineWidth = 1.5;
      ctx.beginPath();

      const mid = canvas.height / 2;
      dataRef.current.forEach((v, i) => {
        const x = (i / dataRef.current.length) * canvas.width;
        const y = mid - v * 1.8;
        if (i === 0) ctx.moveTo(x, y);
        else ctx.lineTo(x, y);
      });
      ctx.stroke();

      animId = requestAnimationFrame(draw);
    };

    draw();
    return () => cancelAnimationFrame(animId);
  }, [bpm]);

  return (
    <canvas
      ref={canvasRef}
      width={280}
      height={64}
      className="w-full h-full"
      style={{ imageRendering: "pixelated" }}
    />
  );
}

function HUDPanel({
  children,
  label,
  className = "",
}: {
  children: ReactNode;
  label?: string;
  className?: string;
}) {
  return (
    <div className={`relative ${className}`}>
      {label && (
        <div
          className="absolute -top-4 left-0 text-[9px] tracking-[0.35em]"
          style={{ color: "rgba(255,255,255,0.25)", fontFamily: "'Space Mono', monospace" }}
        >
          {label}
        </div>
      )}
      <div
        className="relative p-3"
        style={{
          border: "1px solid rgba(255,255,255,0.12)",
          background: "rgba(5,8,15,0.7)",
          backdropFilter: "blur(6px)",
        }}
      >
        <div className="absolute top-0 left-0 w-2 h-2 border-t border-l border-white/30" />
        <div className="absolute top-0 right-0 w-2 h-2 border-t border-r border-white/30" />
        <div className="absolute bottom-0 left-0 w-2 h-2 border-b border-l border-white/30" />
        <div className="absolute bottom-0 right-0 w-2 h-2 border-b border-r border-white/30" />
        {children}
      </div>
    </div>
  );
}

function CompressionGauge({ depth }: { depth: number }) {
  const pct = (depth / 6) * 100;
  const targetMin = (4 / 6) * 100;
  const targetMax = (6 / 6) * 100;
  const inZone = depth >= 4 && depth <= 6;

  return (
    <div className="flex flex-col items-center gap-1 h-full">
      <div
        className="text-[8px] tracking-widest text-center"
        style={{ color: "rgba(255,255,255,0.3)", fontFamily: "'Space Mono', monospace" }}
      >
        PROF.
      </div>

      <div className="relative flex-1 w-6" style={{ minHeight: "100px" }}>
        <div className="absolute inset-0" style={{ border: "1px dashed rgba(255,255,255,0.2)" }} />

        {/* Target zone */}
        <div
          className="absolute left-0 right-0"
          style={{
            bottom: `${targetMin}%`,
            height: `${targetMax - targetMin}%`,
            background: "rgba(255,255,255,0.06)",
            borderTop: "1px dashed rgba(255,255,255,0.2)",
            borderBottom: "1px dashed rgba(255,255,255,0.2)",
          }}
        />

        {/* Fill */}
        <motion.div
          className="absolute bottom-0 left-0 right-0"
          animate={{ height: `${pct}%` }}
          transition={{ duration: 0.1 }}
          style={{
            background: inZone
              ? "rgba(255,255,255,0.35)"
              : depth < 4
              ? "rgba(255,255,255,0.18)"
              : "rgba(255,120,120,0.5)",
          }}
        />

        <div
          className="absolute right-full mr-1 text-[7px]"
          style={{
            bottom: `${targetMax}%`,
            color: "rgba(255,255,255,0.2)",
            fontFamily: "'Space Mono', monospace",
            transform: "translateY(50%)",
          }}
        >
          6cm
        </div>
        <div
          className="absolute right-full mr-1 text-[7px]"
          style={{
            bottom: `${targetMin}%`,
            color: "rgba(255,255,255,0.2)",
            fontFamily: "'Space Mono', monospace",
            transform: "translateY(50%)",
          }}
        >
          4cm
        </div>
      </div>

      <div
        className="text-[10px] tracking-wider text-center"
        style={{
          color: inZone ? "rgba(255,255,255,0.8)" : "rgba(255,255,255,0.4)",
          fontFamily: "'Space Mono', monospace",
        }}
      >
        {depth.toFixed(1)}
      </div>
      <div
        className="text-[7px]"
        style={{ color: "rgba(255,255,255,0.2)", fontFamily: "'Space Mono', monospace" }}
      >
        cm
      </div>
    </div>
  );
}

export function TrainingView() {
  const location = useLocation();
  const isTestMode = location.pathname === "/test";
  const [paused, setPaused] = useState(false);
  const [elapsed, setElapsed] = useState(0);
  const [bpm, setBpm] = useState(102);
  const [depth, setDepth] = useState(0);
  const [compressions, setCompressions] = useState(0);
  const [feedback, setFeedback] = useState<string>("INICIAR COMPRESSÕES");
  const [score, setScore] = useState(0);
  const [compressPhase, setCompressPhase] = useState(0);

  useEffect(() => {
    if (paused) return;
    const interval = setInterval(() => setElapsed((e) => e + 1), 1000);
    return () => clearInterval(interval);
  }, [paused]);

  useEffect(() => {
    if (paused) return;
    let phase = 0;
    const interval = setInterval(() => {
      phase = (phase + 0.08) % (Math.PI * 2);
      setDepth(Math.max(0, Math.sin(phase) * 5.2));
      setCompressPhase(phase);
      if (Math.random() < 0.05) setBpm(Math.floor(95 + Math.random() * 15));
    }, 60);
    return () => clearInterval(interval);
  }, [paused]);

  useEffect(() => {
    if (paused) return;
    const interval = setInterval(() => {
      setCompressions((c) => c + 1);
      setScore((s) => s + Math.floor(Math.random() * 15 + 5));
      const msgs = [
        "BOA PROFUNDIDADE — MANTENHA O RITMO",
        "TAXA: IDEAL",
        "COMPRESSÃO EFICAZ",
        "MANTENHA A POSIÇÃO DAS MÃOS",
        "RITMO CONSISTENTE",
      ];
      setFeedback(msgs[Math.floor(Math.random() * msgs.length)]);
    }, 580);
    return () => clearInterval(interval);
  }, [paused]);

  const formatTime = (s: number) => {
    const m = Math.floor(s / 60);
    const sec = s % 60;
    return `${String(m).padStart(2, "0")}:${String(sec).padStart(2, "0")}`;
  };

  const bpmStatus =
    bpm < 90 ? "MUITO LENTO" : bpm > 115 ? "MUITO RÁPIDO" : "IDEAL";
  const bpmColor =
    bpmStatus === "IDEAL" ? "rgba(255,255,255,0.75)" : "rgba(255,150,100,0.8)";

  const compressDepth = depth;

  return (
    <WFBackground>
      <div className="relative w-full h-full overflow-hidden">
        {/* Mode badge */}
        <div
          className="absolute top-6 left-1/2 -translate-x-1/2 z-10 px-4 py-1 text-[10px] tracking-[0.35em]"
          style={{
            border: "1px dashed rgba(255,255,255,0.2)",
            background: "rgba(5,8,15,0.6)",
            color: isTestMode ? "rgba(255,200,100,0.8)" : "rgba(255,255,255,0.45)",
            fontFamily: "'Space Mono', monospace",
          }}
        >
          {isTestMode ? "// MODO TESTE //" : "// MODO TREINO //"}
        </div>

        {/* ── Patient scene — horizontal mannequin centered ── */}
        <div className="absolute inset-0 flex items-center justify-center">
          <motion.div
            style={{ width: "min(680px, 75vw)", maxWidth: "680px" }}
            animate={{
              y: compressPhase > 0 && compressPhase < Math.PI ? -compressDepth * 0.2 : 0,
            }}
            transition={{ duration: 0.05 }}
          >
            <PatientSilhouette
              highlightChest
              showHandPlacement
              compressDepth={Math.max(0, Math.sin(compressPhase)) * 0.85}
              showArrow={!paused}
            />
          </motion.div>
        </div>

        {/* ─── TOP-LEFT: Indicador de Ritmo / ECG ─── */}
        <div className="absolute top-6 left-6 z-10" style={{ width: "300px" }}>
          <HUDPanel label="INDICADOR DE RITMO">
            <div className="flex flex-col gap-2">
              <div className="flex items-center justify-between mb-1">
                <div className="flex items-center gap-2">
                  <motion.div
                    className="w-2 h-2 rounded-full"
                    style={{ background: "rgba(255,255,255,0.7)" }}
                    animate={{ scale: [1, 1.6, 1], opacity: [0.7, 1, 0.7] }}
                    transition={{ repeat: Infinity, duration: 60 / bpm }}
                  />
                  <span
                    className="text-xs tracking-wider"
                    style={{ color: "rgba(255,255,255,0.7)", fontFamily: "'Space Mono', monospace" }}
                  >
                    {bpm} <span style={{ color: "rgba(255,255,255,0.3)" }}>BPM</span>
                  </span>
                </div>
                <span
                  className="text-[9px] tracking-widest px-2 py-0.5"
                  style={{
                    color: bpmColor,
                    border: `1px dashed ${bpmColor.replace("0.75", "0.3").replace("0.8", "0.3")}`,
                    fontFamily: "'Space Mono', monospace",
                  }}
                >
                  {bpmStatus}
                </span>
              </div>
              <div style={{ height: "64px" }}>
                <ECGDisplay bpm={bpm} />
              </div>
              <div
                className="text-[8px] tracking-widest text-right"
                style={{ color: "rgba(255,255,255,0.18)", fontFamily: "'Space Mono', monospace" }}
              >
                ALVO: 100–110 BPM
              </div>
            </div>
          </HUDPanel>
        </div>

        {/* ─── TOP-RIGHT: Sessão / Timer ─── */}
        <div className="absolute top-6 right-6 z-10" style={{ width: "180px" }}>
          <HUDPanel label="SESSÃO">
            <div className="flex flex-col gap-2">
              <div className="text-center">
                <div
                  className="text-2xl tracking-widest"
                  style={{ color: "rgba(255,255,255,0.85)", fontFamily: "'Space Mono', monospace" }}
                >
                  {formatTime(elapsed)}
                </div>
                <div
                  className="text-[8px] tracking-[0.4em]"
                  style={{ color: "rgba(255,255,255,0.22)", fontFamily: "'Space Mono', monospace" }}
                >
                  DECORRIDO
                </div>
              </div>
              <div className="h-px" style={{ background: "rgba(255,255,255,0.1)" }} />
              <div className="grid grid-cols-2 gap-2">
                <div className="text-center">
                  <div
                    className="text-sm"
                    style={{ color: "rgba(255,255,255,0.7)", fontFamily: "'Space Mono', monospace" }}
                  >
                    {compressions}
                  </div>
                  <div
                    className="text-[7px] tracking-wider"
                    style={{ color: "rgba(255,255,255,0.22)", fontFamily: "'Space Mono', monospace" }}
                  >
                    COMPRESSÕES
                  </div>
                </div>
                {isTestMode ? (
                  <div className="text-center">
                    <div
                      className="text-sm"
                      style={{ color: "rgba(255,220,100,0.8)", fontFamily: "'Space Mono', monospace" }}
                    >
                      {score}
                    </div>
                    <div
                      className="text-[7px] tracking-wider"
                      style={{ color: "rgba(255,255,255,0.22)", fontFamily: "'Space Mono', monospace" }}
                    >
                      PONTUAÇÃO
                    </div>
                  </div>
                ) : (
                  <div className="text-center">
                    <div
                      className="text-sm"
                      style={{ color: "rgba(255,255,255,0.7)", fontFamily: "'Space Mono', monospace" }}
                    >
                      {depth >= 4 ? "✓" : "–"}
                    </div>
                    <div
                      className="text-[7px] tracking-wider"
                      style={{ color: "rgba(255,255,255,0.22)", fontFamily: "'Space Mono', monospace" }}
                    >
                      PROF. OK
                    </div>
                  </div>
                )}
              </div>
            </div>
          </HUDPanel>
        </div>

        {/* ─── BOTTOM-RIGHT: Profundidade de Compressão ─── */}
        <div className="absolute bottom-20 right-6 z-10" style={{ width: "90px" }}>
          <HUDPanel label="COMPRESSÃO">
            <div style={{ height: "160px" }}>
              <CompressionGauge depth={compressDepth} />
            </div>
          </HUDPanel>
        </div>

        {/* ─── BOTTOM-LEFT: Retorno / Feedback ─── */}
        <div className="absolute bottom-20 left-6 z-10" style={{ width: "260px" }}>
          <HUDPanel label="RETORNO">
            <div className="flex flex-col gap-2">
              <AnimatePresence mode="wait">
                <motion.div
                  key={feedback}
                  initial={{ opacity: 0, y: 4 }}
                  animate={{ opacity: 1, y: 0 }}
                  exit={{ opacity: 0, y: -4 }}
                  className="text-xs tracking-widest"
                  style={{ color: "rgba(255,255,255,0.75)", fontFamily: "'Space Mono', monospace" }}
                >
                  {feedback}
                </motion.div>
              </AnimatePresence>

              <div className="flex gap-1 mt-1 flex-wrap">
                {["RITMO", "PROF.", "POSIÇÃO", "TAXA"].map((tag) => (
                  <div
                    key={tag}
                    className="px-1.5 py-0.5 text-[7px] tracking-wider"
                    style={{
                      border: "1px solid rgba(255,255,255,0.12)",
                      color: "rgba(255,255,255,0.3)",
                      fontFamily: "'Space Mono', monospace",
                    }}
                  >
                    {tag}
                  </div>
                ))}
              </div>

              {/* Depth progress bar */}
              <div>
                <div
                  className="text-[7px] tracking-widest mb-1"
                  style={{ color: "rgba(255,255,255,0.22)", fontFamily: "'Space Mono', monospace" }}
                >
                  PROF. ATUAL
                </div>
                <div className="relative h-1.5 w-full" style={{ background: "rgba(255,255,255,0.08)" }}>
                  <motion.div
                    className="absolute left-0 top-0 h-full"
                    animate={{ width: `${(compressDepth / 6) * 100}%` }}
                    transition={{ duration: 0.08 }}
                    style={{
                      background:
                        compressDepth >= 4 ? "rgba(255,255,255,0.6)" : "rgba(255,255,255,0.25)",
                    }}
                  />
                  <div
                    className="absolute top-0 h-full w-px"
                    style={{ left: `${(4 / 6) * 100}%`, background: "rgba(255,255,255,0.3)" }}
                  />
                </div>
              </div>
            </div>
          </HUDPanel>
        </div>

        {/* ─── PAUSE BUTTON ─── */}
        <motion.button
          onClick={() => setPaused(true)}
          whileHover={{ scale: 1.05 }}
          whileTap={{ scale: 0.95 }}
          className="absolute z-20"
          style={{
            top: "50%",
            right: "6px",
            transform: "translateY(-50%)",
            fontFamily: "'Space Mono', monospace",
            background: "rgba(5,8,15,0.7)",
            border: "1px dashed rgba(255,255,255,0.2)",
            color: "rgba(255,255,255,0.4)",
            cursor: "pointer",
            padding: "8px 6px",
            fontSize: "10px",
            letterSpacing: "3px",
            writingMode: "vertical-lr",
            textOrientation: "mixed",
          }}
        >
          ❙❙ PAUSAR
        </motion.button>

        {/* Pause Menu */}
        <PauseMenu
          isOpen={paused}
          onResume={() => setPaused(false)}
          onRestart={() => {
            setPaused(false);
            setElapsed(0);
            setCompressions(0);
            setScore(0);
          }}
        />

        <ScreenNav />
      </div>
    </WFBackground>
  );
}
