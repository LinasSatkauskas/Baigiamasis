const fs = require("fs")
const path = require("path")

const filesToRemove = [
  "Dockerfile",
  "railway.toml",
  "copy-assets.js",
  "AZURE_DEPLOYMENT_GUIDE.md",
  "AZURE_DETAILED_STEPS.md",
]

const dirsToRemove = ["publish"]

filesToRemove.forEach((file) => {
  const filePath = path.join(__dirname, file)
  try {
    if (fs.existsSync(filePath)) {
      fs.unlinkSync(filePath)
      console.log(`✓ Removed: ${file}`)
    }
  } catch (err) {
    console.log(`✗ Failed to remove ${file}: ${err.message}`)
  }
})

dirsToRemove.forEach((dir) => {
  const dirPath = path.join(__dirname, dir)
  try {
    if (fs.existsSync(dirPath)) {
      fs.rmSync(dirPath, { recursive: true, force: true })
      console.log(`✓ Removed directory: ${dir}`)
    }
  } catch (err) {
    console.log(`✗ Failed to remove ${dir}: ${err.message}`)
  }
})

console.log("\n✓ Cleanup complete!")
