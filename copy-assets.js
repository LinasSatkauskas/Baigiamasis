const fs = require("fs")
const path = require("path")

const source = path.join(__dirname, "../reactapp1.client/dist")
const dest = path.join(__dirname, "../ReactApp1.Server/wwwroot")

if (!fs.existsSync(source)) {
  console.error(`Source folder not found: ${source}`)
  process.exit(1)
}

// Clear destination
if (fs.existsSync(dest)) {
  fs.rmSync(dest, { recursive: true })
}

// Copy
function copyDir(src, dst) {
  if (!fs.existsSync(dst)) fs.mkdirSync(dst, { recursive: true })

  fs.readdirSync(src).forEach((file) => {
    const srcFile = path.join(src, file)
    const dstFile = path.join(dst, file)

    if (fs.statSync(srcFile).isDirectory()) {
      copyDir(srcFile, dstFile)
    } else {
      fs.copyFileSync(srcFile, dstFile)
    }
  })
}

copyDir(source, dest)
console.log(`✓ Copied frontend assets to ${dest}`)
