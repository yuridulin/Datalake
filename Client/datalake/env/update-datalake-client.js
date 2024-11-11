import { promises as fs } from 'fs'
import path from 'path'
import { fileURLToPath } from 'url'

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);

async function deleteFilesAndFolders(dir) {
    const entries = await fs.readdir(dir, { withFileTypes: true });

    const deletePromises = entries.map(async (entry) => {
        const fullPath = path.join(dir, entry.name);
        if (entry.isDirectory()) {
            await deleteFilesAndFolders(fullPath);
            await fs.rmdir(fullPath);
        } else {
            await fs.unlink(fullPath);
        }
    });

    await Promise.all(deletePromises);
}

async function copyFiles(srcDir, destDir) {
    const entries = await fs.readdir(srcDir, { withFileTypes: true });

    await fs.mkdir(destDir, { recursive: true });

    const copyPromises = entries.map(async (entry) => {
        const srcPath = path.join(srcDir, entry.name);
        const destPath = path.join(destDir, entry.name);

        if (entry.isDirectory()) {
            await copyFiles(srcPath, destPath);
        } else {
            await fs.copyFile(srcPath, destPath);
        }
    });

    await Promise.all(copyPromises);
}

const targetDir = path.resolve(__dirname, '../../../Server/wwwroot');
const sourceDir = path.resolve(__dirname, '../dist');

deleteFilesAndFolders(targetDir)
    .then(() => copyFiles(sourceDir, targetDir))
    .then(() => console.log('Client updated, all OK'))
    .catch((err) => console.error('ERR:', err));
