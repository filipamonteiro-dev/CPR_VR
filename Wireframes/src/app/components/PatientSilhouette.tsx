import { motion } from "motion/react";

interface PatientSilhouetteProps {
  highlightChest?: boolean;
  showHandPlacement?: boolean;
  compressDepth?: number; // 0-1
  showArrow?: boolean;
}

// Chest zone center (for hands and arrow anchor)
const CX = 218;
const CY = 104;

export function PatientSilhouette({
  highlightChest = false,
  showHandPlacement = false,
  compressDepth = 0,
  showArrow = false,
}: PatientSilhouetteProps) {
  // Chest sinks slightly when compressed
  const sinkY = compressDepth * 5;

  return (
    <svg
      viewBox="0 0 700 210"
      className="w-full h-full"
      style={{ maxWidth: "700px", maxHeight: "210px" }}
    >
      {/* ── Ground / floor surface ── */}
      <line
        x1="10" y1="158" x2="690" y2="158"
        stroke="rgba(255,255,255,0.14)" strokeWidth="1" strokeDasharray="10,7"
      />
      <line
        x1="10" y1="160" x2="690" y2="160"
        stroke="rgba(255,255,255,0.04)" strokeWidth="1"
      />
      {/* Shadow beneath body */}
      <ellipse
        cx="345" cy="158" rx="300" ry="6"
        fill="rgba(0,0,0,0)"
        stroke="rgba(255,255,255,0.04)" strokeWidth="0"
      />

      {/* ── LEFT ARM (above body) ── */}
      <rect
        x="122" y="47" width="192" height="22" rx="10"
        fill="none" stroke="rgba(255,255,255,0.18)" strokeWidth="1.2"
      />

      {/* ── RIGHT ARM (below body) ── */}
      <rect
        x="122" y="135" width="192" height="22" rx="10"
        fill="none" stroke="rgba(255,255,255,0.18)" strokeWidth="1.2"
      />

      {/* ── HEAD ── */}
      <ellipse
        cx="60" cy="104" rx="42" ry="34"
        fill="none" stroke="rgba(255,255,255,0.38)" strokeWidth="1.5"
      />
      {/* Facial cross-guide (wireframe detail) */}
      <line x1="60" y1="78" x2="60" y2="130" stroke="rgba(255,255,255,0.07)" strokeWidth="0.7" strokeDasharray="3,5" />
      <line x1="30" y1="104" x2="90" y2="104" stroke="rgba(255,255,255,0.07)" strokeWidth="0.7" strokeDasharray="3,5" />

      {/* ── NECK ── */}
      <rect
        x="100" y="96" width="20" height="16" rx="2"
        fill="none" stroke="rgba(255,255,255,0.22)" strokeWidth="1.1"
      />

      {/* ── SHOULDER TRAPEZOID ── */}
      <path
        d="M118,96 Q130,82 165,82 L165,128 Q130,128 118,116 Z"
        fill="none" stroke="rgba(255,255,255,0.22)" strokeWidth="1.1"
      />

      {/* ── TORSO (main body rectangle) ── */}
      <rect
        x="163" y="82" width="175" height="46" rx="3"
        fill="none" stroke="rgba(255,255,255,0.30)" strokeWidth="1.5"
      />

      {/* Rib lines (decorative horizontal) */}
      {[92, 100, 108, 116].map((y, i) => (
        <line
          key={i}
          x1="168" y1={y} x2="333" y2={y}
          stroke="rgba(255,255,255,0.055)" strokeWidth="0.8" strokeDasharray="4,9"
        />
      ))}
      {/* Sternum center line (vertical) */}
      <line
        x1={CX} y1="83" x2={CX} y2="127"
        stroke="rgba(255,255,255,0.08)" strokeWidth="0.8" strokeDasharray="3,5"
      />

      {/* ── PELVIS / HIP TRANSITION ── */}
      <path
        d="M337,82 L370,86 L375,104 L370,122 L337,128 Z"
        fill="none" stroke="rgba(255,255,255,0.22)" strokeWidth="1.2"
      />

      {/* ── LEFT LEG (upper — closer to viewer) ── */}
      <rect
        x="373" y="86" width="295" height="18" rx="8"
        fill="none" stroke="rgba(255,255,255,0.20)" strokeWidth="1.2"
      />
      {/* ── RIGHT LEG (lower) ── */}
      <rect
        x="373" y="106" width="295" height="18" rx="8"
        fill="none" stroke="rgba(255,255,255,0.20)" strokeWidth="1.2"
      />
      {/* Knee joint markers */}
      <line x1="520" y1="86" x2="520" y2="104" stroke="rgba(255,255,255,0.08)" strokeWidth="0.8" />
      <line x1="520" y1="106" x2="520" y2="124" stroke="rgba(255,255,255,0.08)" strokeWidth="0.8" />

      {/* ── CHEST COMPRESSION TARGET ZONE ── */}
      {highlightChest && (
        <g>
          {/* Glowing fill behind zone */}
          <motion.rect
            x={CX - 48}
            y={82 + sinkY}
            width="96"
            height="46"
            rx="2"
            fill="rgba(255,255,255,0.04)"
            animate={{ opacity: [0.4, 0.9, 0.4] }}
            transition={{ repeat: Infinity, duration: 1.4 }}
          />
          {/* Dashed border highlight */}
          <motion.rect
            x={CX - 48}
            y={82 + sinkY}
            width="96"
            height="46"
            rx="2"
            fill="none"
            stroke="rgba(255,255,255,0.70)"
            strokeWidth="1.6"
            strokeDasharray="5,3"
            animate={{ opacity: [0.55, 1, 0.55] }}
            transition={{ repeat: Infinity, duration: 1.4 }}
          />
          {/* Corner brackets on zone */}
          <line x1={CX - 54} y1={82 + sinkY} x2={CX - 10} y2={82 + sinkY} stroke="rgba(255,255,255,0.28)" strokeWidth="0.8" />
          <line x1={CX + 10} y1={82 + sinkY} x2={CX + 54} y2={82 + sinkY} stroke="rgba(255,255,255,0.28)" strokeWidth="0.8" />
          <line x1={CX - 54} y1={128 + sinkY} x2={CX - 10} y2={128 + sinkY} stroke="rgba(255,255,255,0.28)" strokeWidth="0.8" />
          <line x1={CX + 10} y1={128 + sinkY} x2={CX + 54} y2={128 + sinkY} stroke="rgba(255,255,255,0.28)" strokeWidth="0.8" />

          {/* Crosshair dot at center */}
          <motion.circle
            cx={CX} cy={CY + sinkY} r="4"
            fill="none" stroke="rgba(255,255,255,0.5)" strokeWidth="1"
            animate={{ r: [4, 7, 4], opacity: [0.5, 0.15, 0.5] }}
            transition={{ repeat: Infinity, duration: 1.4 }}
          />
          <circle cx={CX} cy={CY + sinkY} r="2" fill="rgba(255,255,255,0.6)" />

          {/* Label above zone */}
          <text
            x={CX} y={70 + sinkY}
            textAnchor="middle"
            fill="rgba(255,255,255,0.55)"
            fontSize="8"
            fontFamily="'Space Mono', monospace"
            letterSpacing="2.5"
          >
            ZONA DE COMPRESSÃO
          </text>
          {/* Small tick up from label */}
          <line x1={CX} y1={72 + sinkY} x2={CX} y2={82 + sinkY} stroke="rgba(255,255,255,0.2)" strokeWidth="0.8" />
        </g>
      )}

      {/* ── HAND PLACEMENT OVERLAY ── */}
      {showHandPlacement && (
        <g>
          {/* Bottom hand (heel of hand — larger) */}
          <ellipse
            cx={CX} cy={CY - 5 + sinkY}
            rx="30" ry="11"
            fill="rgba(255,255,255,0.06)"
            stroke="rgba(255,255,255,0.50)" strokeWidth="1.3" strokeDasharray="3,2.5"
          />
          {/* Top hand stacked */}
          <ellipse
            cx={CX} cy={CY - 14 + sinkY}
            rx="25" ry="9"
            fill="rgba(255,255,255,0.03)"
            stroke="rgba(255,255,255,0.35)" strokeWidth="1.1" strokeDasharray="3,2.5"
          />
          {/* Label */}
          <text
            x={CX + 38} y={CY - 6 + sinkY}
            fill="rgba(255,255,255,0.40)"
            fontSize="7"
            fontFamily="'Space Mono', monospace"
            letterSpacing="1"
          >
            MÃOS
          </text>
        </g>
      )}

      {/* ── COMPRESSION ARROW (pointing DOWN into chest) ── */}
      {showArrow && (
        <motion.g
          animate={{ y: [0, compressDepth * 7, 0] }}
          transition={{ repeat: Infinity, duration: 0.58, ease: "easeInOut" }}
        >
          {/* Shaft */}
          <line
            x1={CX} y1={42 + sinkY}
            x2={CX} y2={72 + sinkY}
            stroke="rgba(255,255,255,0.65)" strokeWidth="1.5"
          />
          {/* Arrowhead */}
          <polygon
            points={`${CX - 7},${68 + sinkY} ${CX + 7},${68 + sinkY} ${CX},${80 + sinkY}`}
            fill="rgba(255,255,255,0.60)"
          />
          {/* Action label */}
          <text
            x={CX + 12} y={55 + sinkY}
            fill="rgba(255,255,255,0.38)"
            fontSize="7"
            fontFamily="'Space Mono', monospace"
            letterSpacing="1.5"
          >
            PRESSIONAR
          </text>
        </motion.g>
      )}

      {/* ── GROUND LABEL ── */}
      <text
        x="350" y="175"
        textAnchor="middle"
        fill="rgba(255,255,255,0.10)"
        fontSize="7.5"
        fontFamily="'Space Mono', monospace"
        letterSpacing="3"
      >
        SUPERFÍCIE · ELEVAÇÃO: 0,0m
      </text>
    </svg>
  );
}
