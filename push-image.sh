#!/usr/bin/env bash
#
# Build a Docker image and push it to a registry, tagged with the last commit's
# short hash (and :latest). Nothing is hardcoded — everything is passed as a flag
# (or an env var). Run it from the repo whose image you want to build.
#
set -euo pipefail

# All configurable values; env vars act as fallbacks, flags override them.
REGISTRY="${REGISTRY:-}"
IMAGE="${IMAGE:-castit}"
DOCKERFILE="${DOCKERFILE:-CastIt.Server/Dockerfile}"
PLATFORM="${PLATFORM:-linux/arm64}"
CONTEXT="${CONTEXT:-.}"
PUSH_LATEST=1

usage() {
  cat >&2 <<'EOF'
Usage: ./push-image.sh --registry <host> --image <name> [options]

  --registry <host>     Registry host (env: REGISTRY)                    [required]
  --image <name>        Image repository name (env: IMAGE, default: castit)
  --dockerfile <path>   Dockerfile path (env: DOCKERFILE, default: CastIt.Server/Dockerfile)
  --platform <p>        Target platform (env: PLATFORM, default: linux/arm64)
  --context <dir>       Build context (env: CONTEXT, default: .)
  --sha-only            Push only the :<sha> tag (skip :latest)
  -h, --help            Show this help
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    --registry)   REGISTRY="$2"; shift 2 ;;
    --image)      IMAGE="$2"; shift 2 ;;
    --dockerfile) DOCKERFILE="$2"; shift 2 ;;
    --platform)   PLATFORM="$2"; shift 2 ;;
    --context)    CONTEXT="$2"; shift 2 ;;
    --sha-only)   PUSH_LATEST=0; shift ;;
    -h|--help)    usage; exit 0 ;;
    *) echo "Unknown argument: $1" >&2; usage; exit 1 ;;
  esac
done

[[ -n "$REGISTRY" ]] || { echo "ERROR: --registry (or REGISTRY env) is required" >&2; exit 1; }

# Always run from the repo root (where this script lives)
cd "$(dirname "$0")"

# Tag = last commit's short hash
SHA="$(git rev-parse --short HEAD)"
REF="${REGISTRY}/${IMAGE}"

# Warn if there are uncommitted changes — the tag won't reflect them
if ! git diff --quiet || ! git diff --cached --quiet; then
  echo "WARNING: working tree has uncommitted changes; tagging as ${SHA} anyway." >&2
fi

# Make sure we're authenticated to the registry
if ! grep -q "${REGISTRY}" "${HOME}/.docker/config.json" 2>/dev/null; then
  echo "Not logged in to ${REGISTRY} — running docker login..."
  docker login "${REGISTRY}"
fi

# Build tag list
TAGS=(-t "${REF}:${SHA}")
[[ "$PUSH_LATEST" -eq 1 ]] && TAGS+=(-t "${REF}:latest")

echo "Building and pushing ${REF}:${SHA} (${PLATFORM})..."
docker buildx build \
  --platform "${PLATFORM}" \
  --build-arg "TARGETARCH=${PLATFORM##*/}" \
  "${TAGS[@]}" \
  -f "${DOCKERFILE}" \
  --push \
  "${CONTEXT}"

echo
echo "Pushed:"
echo "  ${REF}:${SHA}"
[[ "$PUSH_LATEST" -eq 1 ]] && echo "  ${REF}:latest"
